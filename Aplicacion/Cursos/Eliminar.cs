using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Persistencia;
using Aplicacion.ManejadorError;
using System.Linq;

namespace Aplicacion.Cursos
{
    public class Eliminar
    {
        public class Ejecuta : IRequest
        {
            public Guid Id { get; set; }
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
                var curso = await _context.Curso.FindAsync(request.Id);
                if (curso == null)
                {
                    // throw new Exception("No se puede eliminar el curso");
                    throw new ManejadorExcepcion(HttpStatusCode.NotFound, new { mensaje = "No se encontro el curso" });
                }

                var instructoresDB = _context.CursoInstructor.Where(x => x.CursoId == request.Id).ToList();
                if (instructoresDB != null && instructoresDB.Count > 0)
                {
                    foreach (var instructor in instructoresDB)
                    {
                        _context.Remove(instructor);
                    }
                }

                var comentariosDB = _context.Comentario.Where(x => x.ComentarioId == request.Id).ToList();
                if (comentariosDB != null && comentariosDB.Count > 0)
                {
                    foreach (var comentario in comentariosDB)
                    {
                        _context.Remove(comentario);
                    }
                }

                //remover precio
                var precioDb = _context.Precio.FirstOrDefault(x => x.CursoId == curso.CursoId);
                if (precioDb != null)
                {
                    _context.Precio.Remove(precioDb);
                }

                _context.Remove(curso);


                var resultado = await _context.SaveChangesAsync();
                if (resultado > 0)
                {
                    return Unit.Value;
                }

                // throw new Exception("No se pudieron guardar los cambios");
                throw new ManejadorExcepcion(HttpStatusCode.NotFound, new { mensaje = "No se pudieron guardar los cambios" });
            }
        }
    }
}