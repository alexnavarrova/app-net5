using System.Collections.Generic;
using System.Net;
using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using Persistencia;
using Aplicacion.ManejadorError;
using System.Linq;
using Dominio;

namespace Aplicacion.Cursos
{
    public class Editar
    {
        public class Ejecuta : IRequest
        {
            public Guid CursoId { get; set; }
            public string Titulo { get; set; }
            public string Descripcion { get; set; }
            public DateTime? FechaPublicacion { get; set; }
            public List<Guid> ListaInstructor { get; set; }
            public decimal? Precio { get; set; }
            public decimal? Promocion { get; set; }
        }

        public class EjecutaValidacion : AbstractValidator<Ejecuta>
        {
            public EjecutaValidacion()
            {
                RuleFor(x => x.Titulo).NotEmpty();
                RuleFor(x => x.Descripcion).NotEmpty();
                RuleFor(x => x.FechaPublicacion).NotEmpty();
                RuleFor(x => x.Precio).NotEmpty();
                RuleFor(x => x.Promocion).NotEmpty();
            }
        }

        public class Manejador : IRequestHandler<Ejecuta>
        {
            private readonly CursosOnlineContext _context;

            public Manejador(CursosOnlineContext context)
            {
                _context = context;
            }

            public async Task<Unit> Handle(Ejecuta request, CancellationToken cancellationToken)
            {
                var curso = await _context.Curso.FindAsync(request.CursoId);
                if (curso == null)
                {
                    // throw new Exception("El curso no existe");
                    throw new ManejadorExcepcion(HttpStatusCode.NotFound, new { mensaje = "No se encontro el curso" });
                }

                curso.Titulo = request.Titulo ?? curso.Titulo;
                curso.Descripcion = request.Descripcion ?? curso.Descripcion;
                curso.FechaPublicacion = request.FechaPublicacion ?? curso.FechaPublicacion;

                if (request.ListaInstructor != null && request.ListaInstructor.Count > 0)
                {
                    /*Eliminar los instructores actuales del curso en la base de datos*/
                    var instructoresBD = _context.CursoInstructor.Where(x => x.CursoId == request.CursoId).ToList();
                    foreach (var instructorEliminar in instructoresBD)
                    {
                        _context.CursoInstructor.Remove(instructorEliminar);
                    }
                    //Fin del procedimiento para eliminar instructores

                    //Procedimiento para agregar instrcutores que provienen del cliente
                    foreach (var ids in request.ListaInstructor)
                    {
                        var nuevoInstructor = new CursoInstructor
                        {
                            CursoId = request.CursoId,
                            InstructorId = ids
                        };
                        _context.CursoInstructor.Add(nuevoInstructor);
                    }
                    //Fin del procedimiento

                    //Actualizar el precio del curso
                    var precioEntidad = _context.Precio.FirstOrDefault(x => x.CursoId == curso.CursoId);
                    if (precioEntidad != null)
                    {
                        precioEntidad.PercioActual = request.Precio ?? precioEntidad.PercioActual;
                        precioEntidad.Promocion = request.Promocion ?? precioEntidad.Promocion;
                        _context.Precio.Update(precioEntidad);
                    }
                    else
                    {
                        precioEntidad = new Precio
                        {
                            PrecioId = Guid.NewGuid(),
                            PercioActual = request.Precio ?? 0,
                            Promocion = request.Promocion ?? 0,
                            CursoId = curso.CursoId
                        };
                        await _context.Precio.AddAsync(precioEntidad);
                    }
                }

                int resultado = await _context.SaveChangesAsync();
                if (resultado > 0)
                {
                    return Unit.Value;
                };
                throw new Exception("No se guardaron los cambios");
            }
        }


    }
}