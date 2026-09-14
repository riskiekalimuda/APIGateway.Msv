using APIGateway.Msv.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGatewayServices(builder.Configuration);

var app = builder.Build();


app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapReverseProxy();
app.Run();

