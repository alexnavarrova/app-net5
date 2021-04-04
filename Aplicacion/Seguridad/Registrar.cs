using System.Data;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Aplicacion.Contratos;
using Aplicacion.ManejadorError;
using Dominio;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistencia;

namespace Aplicacion.Seguridad
{
    public class Registrar
    {
        public class Ejecuta : IRequest<UsuarioData>
        {
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string Email { get; set; }
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        public class EjecutaValidado : AbstractValidator<Ejecuta>
        {
            public EjecutaValidado()
            {
                RuleFor(x => x.Nombre).NotEmpty().WithMessage("El nombre es requerido");
                RuleFor(x => x.Apellido).NotEmpty();
                RuleFor(x => x.Email).NotEmpty().EmailAddress();
                RuleFor(x => x.Password).NotEmpty();
                RuleFor(x => x.UserName).NotEmpty();
            }
        }

        public class Manejador : IRequestHandler<Ejecuta, UsuarioData>
        {
            private readonly CursosOnlineContext _context;
            private readonly UserManager<Usuario> _userManager;
            private readonly IJwtGenerador _jwtGenerador;
            public Manejador(CursosOnlineContext context, UserManager<Usuario> userManager, IJwtGenerador jwtGenerador)
            {
                _context = context;
                _userManager = userManager;
                _jwtGenerador = jwtGenerador;
            }
            public async Task<UsuarioData> Handle(Ejecuta request, CancellationToken cancellationToken)
            {
                var existe = await _context.Users.Where(x => x.Email.Equals(request.Email)).AnyAsync();
                if (existe)
                {
                    throw new ManejadorExcepcion(HttpStatusCode.BadRequest, new { mensaje = "Existe ya un usuario registrado con este email" });
                }

                var existeUserName = await _context.Users.Where(x => x.UserName.Equals(request.UserName)).AnyAsync();

                if (existeUserName)
                {
                    throw new ManejadorExcepcion(HttpStatusCode.BadRequest, new { mensaje = "Existe ya un usuario con este username" });
                }

                var usuario = new Usuario
                {
                    NombreCompleto = request.Nombre + " " + request.Apellido,
                    Email = request.Email,
                    UserName = request.UserName
                };

                var resultado = await _userManager.CreateAsync(usuario, request.Password);
                if (resultado.Succeeded)
                {
                    return new UsuarioData
                    {
                        NombreCompleto = usuario.NombreCompleto,
                        Token = _jwtGenerador.CrearToken(usuario),
                        Email = usuario.Email,
                        UserName = usuario.UserName

                    };
                }

                throw new Exception("No se pudo agregar al nuevo usuario");
            }
        }
    }
}