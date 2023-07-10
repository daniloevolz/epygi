﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Text.Json;
using WebApplicationWecomEpygi.Models;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using BCrypt.Net;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Security.Principal;
using System.Security.Claims;
using System.Xml.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace WebApplicationWecomEpygi.Controllers
{
    public class HomeController : ControllerBase
    {
        private readonly IJwtConfig _jwtConfig;


        public HomeController(IJwtConfig jwtConfig)
        {
            _jwtConfig = jwtConfig;
        }

        [HttpPost]
        public IActionResult Login([FromBody] LoginData model)
        {
            try
            {
                // Carrega os dados de login do arquivo password.json
                var passwordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "password.json");
                var json = System.IO.File.ReadAllText(passwordFilePath);
                var loginData = JsonSerializer.Deserialize<LoginData[]>(json);

                // Procura pelo usuário correspondente ao nome de usuário fornecido
                var user = loginData.FirstOrDefault(u => u.Username == model.Username);
                string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash.ToString()); ;
                if (user != null && model.PasswordHash == user.PasswordHash)
                {
                    // Autenticação bem-sucedida
                    // Gerar o token JWT
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);

                    var claims = new List<Claim>
                    {
                        new Claim("Username", model.Username),
                        new Claim("Password", model.PasswordHash),
                    };

                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Issuer = _jwtConfig.Issuer,
                        Expires = DateTime.UtcNow.AddMinutes(10),// AddDays(1),
                        Subject = new ClaimsIdentity(claims),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                    };
                    var token = tokenHandler.CreateToken(tokenDescriptor);
                    var tokenString = tokenHandler.WriteToken(token);


                    return Ok(new { success = tokenString, message = "Login bem-sucedido." });
                }

                // Credenciais inválidas
                return BadRequest(new { message = "Credenciais inválidas." });

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Erro " + ex.Message });
            }

        }

        [HttpPost]
        [Authorize]
        public IActionResult AddLogin([FromBody] LoginData model)
        {
            try
            {
                // Verifique o token JWT
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                if (validatedToken != null && principal.Identity.IsAuthenticated)
                {
                    // Verificar se as reivindicações necessárias estão presentes no token
                    if (principal.HasClaim(c => c.Type == "Username") &&
                        principal.HasClaim(c => c.Type == "Password"))
                    {
                        // Extrair os dados das reivindicações
                        string Username = principal.FindFirstValue("Username");
                        string Password = principal.FindFirstValue("Password");

                        bool valid = ValidateUser(Username, Password);
                        if (valid)
                        {
                            // Carrega os dados de login existentes do arquivo password.json
                            var passwordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "password.json");
                            var json = System.IO.File.ReadAllText(passwordFilePath);
                            var loginData = JsonSerializer.Deserialize<LoginData[]>(json)?.ToList() ?? new List<LoginData>();

                            // Verifica se o nome de usuário já existe
                            if (loginData.Any(u => u.Username == model.Username))
                            {
                                return BadRequest(new { message = "Nome de usuário já existe." });
                            }

                            // Hashear a senha usando o BCrypt
                            //var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash.ToString());

                            // Adicionar o novo objeto de login
                            loginData.Add(new LoginData
                            {
                                Username = model.Username,
                                PasswordHash = model.PasswordHash
                            });

                            // Serializar os dados atualizados
                            var updatedJson = JsonSerializer.Serialize(loginData, new JsonSerializerOptions
                            {
                                WriteIndented = true
                            });

                            // Escrever os dados atualizados no arquivo password.json
                            System.IO.File.WriteAllText(passwordFilePath, updatedJson);

                            return Ok(new { message = "Login adicionado com sucesso." });

                        }
                    }

                }
                return BadRequest(new { message = "Não autorizado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPost]
        [Authorize]
        public IActionResult DelLogin([FromBody] List<string> logins)
        {
            try
            {
                // Verifique o token JWT
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                if (validatedToken != null && principal.Identity.IsAuthenticated)
                {
                    // Verificar se as reivindicações necessárias estão presentes no token
                    if (principal.HasClaim(c => c.Type == "Username") &&
                        principal.HasClaim(c => c.Type == "Password"))
                    {
                        // Extrair os dados das reivindicações
                        string Username = principal.FindFirstValue("Username");
                        string Password = principal.FindFirstValue("Password");

                        bool valid = ValidateUser(Username, Password);
                        if (valid)
                        {
                            // Carrega os dados de login existentes do arquivo password.json
                            var passwordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "password.json");

                            // Verificar se o arquivo existe
                            if (System.IO.File.Exists(passwordFilePath))
                            {
                                // Ler o conteúdo do arquivo
                                string usersJson = System.IO.File.ReadAllText(passwordFilePath);

                                // Desserializar o conteúdo em uma lista de usuários
                                List<LoginData> users = JsonSerializer.Deserialize<List<LoginData>>(usersJson);

                                // Remover os usuários correspondentes ao parâmetro sip ["sip1","sip2","sip3"]
                                users.RemoveAll(u => logins.Contains(u.Username));

                                // Serializar a lista de usuários atualizada em JSON
                                string usersJsonUpdated = JsonSerializer.Serialize(users);

                                // Gravar a lista de usuários atualizada no arquivo users.json
                                System.IO.File.WriteAllText(passwordFilePath, usersJsonUpdated);
                            }

                            return Ok(new { message = "Login removido com sucesso." });

                        }
                    }

                }
                return BadRequest(new { message = "Não autorizado" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }

        }

        [HttpPost]
        [Authorize]
        public IActionResult AddUser([FromBody] JsonElement data)
        {
            try
            {
                // Verifique o token JWT
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                if (validatedToken != null && principal.Identity.IsAuthenticated)
                {
                    // Verificar se as reivindicações necessárias estão presentes no token
                    if (principal.HasClaim(c => c.Type == "Username") &&
                        principal.HasClaim(c => c.Type == "Password"))
                    {
                        // Extrair os dados das reivindicações
                        string Username = principal.FindFirstValue("Username");
                        string Password = principal.FindFirstValue("Password");

                        bool valid = ValidateUser(Username, Password);
                        if (valid)
                        {
                            // Extrair os dados do JSON
                            string name = data.GetProperty("user").GetProperty("name").GetString();
                            string sip = data.GetProperty("user").GetProperty("sip").GetString();
                            string num = data.GetProperty("user").GetProperty("num").GetString();
                            string password = data.GetProperty("user").GetProperty("pass").GetString();
                            string email = data.GetProperty("user").GetProperty("email").GetString();
                            string location = data.GetProperty("user").GetProperty("location").GetString();
                            string department = data.GetProperty("user").GetProperty("department").GetString();
                            string imageName = sip + "-user.jpg";
                            string imageUrl = "./images/" + imageName;
                            string imageSize = data.GetProperty("image").GetProperty("size").GetUInt64().ToString();
                            string imageData = data.GetProperty("image").GetProperty("data").GetString();

                            // Carregar os usuários existentes do arquivo users.json (se existir)
                            string usersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "users.json");
                            List<User> users = new List<User>();

                            if (System.IO.File.Exists(usersFilePath))
                            {
                                string usersJson = System.IO.File.ReadAllText(usersFilePath);
                                try
                                {
                                    users = JsonSerializer.Deserialize<List<User>>(usersJson);

                                }
                                catch (Exception ex)
                                {

                                }

                            }

                            // Criar um novo objeto User
                            User newUser = new User
                            {
                                name = name,
                                sip = sip,
                                num = num,
                                department = department,
                                location = location,
                                email = email,
                                img = imageUrl
                            };

                            // Adicionar o novo usuário à lista
                            users.Add(newUser);

                            // Serializar a lista de usuários em JSON
                            string usersJsonUpdated = JsonSerializer.Serialize(users);

                            // Gravar a lista de usuários atualizada no arquivo users.json
                            System.IO.File.WriteAllText(usersFilePath, usersJsonUpdated);

                            // Salvar a imagem em /StaticFiles/images/
                            string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "images");
                            string imagePath = Path.Combine(imagesDirectory, imageName);
                            string base64String = imageData.Split(',')[1]; // Remove o prefixo "data:image/jpeg;base64,"
                            byte[] imageBytes = Convert.FromBase64String(base64String);
                            System.IO.File.WriteAllBytes(imagePath, imageBytes);

                            var result = InsertAgentsJSON(name, sip, num, password);
                            if (result.success == true)
                            {
                                return Ok(new { success = true, message = "Usuário adicionado com sucesso." });
                            }
                            else
                            {
                                return BadRequest(new { success = false, message = "Erro ao adicionar usuário: " + result.message });
                            }


                        }
                    }

                }
                return BadRequest(new { success = false, message = "Não autorizado!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Erro ao adicionar usuário: " + ex.Message });
            }
        }
        [HttpPost]
        [Authorize]
        public IActionResult DelUser([FromBody] List<string> sips)
        {
            try
            {
                // Verifique o token JWT
                string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_jwtConfig.SecretKey);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                if (validatedToken != null && principal.Identity.IsAuthenticated)
                {
                    // Verificar se as reivindicações necessárias estão presentes no token
                    if (principal.HasClaim(c => c.Type == "Username") &&
                        principal.HasClaim(c => c.Type == "Password"))
                    {
                        // Extrair os dados das reivindicações
                        string Username = principal.FindFirstValue("Username");
                        string Password = principal.FindFirstValue("Password");

                        bool valid = ValidateUser(Username, Password);
                        if (valid)
                        {
                            // Caminho do arquivo users.json
                            string usersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "users.json");

                            // Verificar se o arquivo existe
                            if (System.IO.File.Exists(usersFilePath))
                            {
                                // Ler o conteúdo do arquivo
                                string usersJson = System.IO.File.ReadAllText(usersFilePath);

                                // Desserializar o conteúdo em uma lista de usuários
                                List<User> users = JsonSerializer.Deserialize<List<User>>(usersJson);

                                // Remover os usuários correspondentes ao parâmetro sip ["sip1","sip2","sip3"]
                                users.RemoveAll(u => sips.Contains(u.sip));

                                // Serializar a lista de usuários atualizada em JSON
                                string usersJsonUpdated = JsonSerializer.Serialize(users);

                                // Gravar a lista de usuários atualizada no arquivo users.json
                                System.IO.File.WriteAllText(usersFilePath, usersJsonUpdated);
                            }

                            var result = DeleteAgentsJSON(sips);
                            if (result.success)
                            {
                                return Ok(new { success = true, message = "Usuários removidos dos Cards e da lista de Agents 3PCC. Serviço reiniciado!" });
                            }
                            else
                            {
                                return Ok(new { success = false, message = "Usuários removidos dos Cards. Ocorreu um erro ao excluir a lista de Agents 3CPP: " + result.message });
                            }
                        }
                    }

                }
                return BadRequest(new { success = false, message = "Não autorizado!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = "Erro ao remover usuários: " + ex.Message });
            }
        }


        public static string DecodeMd5Hash(string hashedString)
        {
            using (var md5 = MD5.Create())
            {
                // Converte a string hasheada para bytes
                byte[] hashBytes = new byte[hashedString.Length / 2];
                for (int i = 0; i < hashedString.Length; i += 2)
                {
                    hashBytes[i / 2] = Convert.ToByte(hashedString.Substring(i, 2), 16);
                }

                // Calcula o hash reverso para obter a senha original (não é possível obter a senha real, apenas um valor que gere o mesmo hash MD5)
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    hashBytes[i] = (byte)~hashBytes[i];
                }

                // Converte os bytes para string
                string decodedString = Encoding.UTF8.GetString(hashBytes);

                return decodedString;
            }
        }
        public static bool ValidateUser(string username, string password)
        {
            bool result = false;
            try
            {
                // Carrega os dados de login do arquivo password.json
                var passwordFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "password.json");
                var json = System.IO.File.ReadAllText(passwordFilePath);
                var loginData = JsonSerializer.Deserialize<LoginData[]>(json);

                // Procura pelo usuário correspondente ao nome de usuário fornecido
                var user = loginData.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);
                if (user != null)
                {
                    result = true;
                }
                return result;
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public static dynamic RestartService()
        {
            try
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

                string nomeServico = config["ServiceName"];


                using (var controller = new ServiceController(nomeServico))
                {
                    if (controller.Status == ServiceControllerStatus.Running)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
                    }

                    controller.Start();
                    controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
                }
                return new { success = true, message = "Serviço reiniciado com sucesso" };
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }

        }

        public static dynamic InsertAgentsJSON(string name, string sip, string num, string password)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            var AgentsJSONPatch = config["AgentsJSONPatch"];
            try
            {
                // Carregar os usuários existentes do arquivo users.json (se existir)
                string usersFilePath = AgentsJSONPatch; //Path.Combine(Directory.GetCurrentDirectory(), "Service", "agents.json");
                List<Agent> agents = new List<Agent>();

                if (System.IO.File.Exists(usersFilePath))
                {
                    string usersJson = System.IO.File.ReadAllText(usersFilePath);
                    try
                    {
                        agents = JsonSerializer.Deserialize<List<Agent>>(usersJson);

                    }
                    catch (Exception ex)
                    {

                    }

                }

                // Criar um novo objeto User
                Agent newUser = new Agent
                {
                    Name = name,
                    Sip = sip,
                    Num = num,
                    Password = password
                };

                // Adicionar o novo usuário à lista
                agents.Add(newUser);

                // Serializar a lista de usuários em JSON
                string usersJsonUpdated = JsonSerializer.Serialize(agents);

                // Gravar a lista de usuários atualizada no arquivo users.json
                System.IO.File.WriteAllText(usersFilePath, usersJsonUpdated);

                return RestartService();
            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }


        }

        public static dynamic DeleteAgentsJSON(List<string> sips)
        {
            try
            {
                var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
                var AgentsJSONPatch = config["AgentsJSONPatch"];
                // Caminho do arquivo users.json
                string usersFilePath = AgentsJSONPatch; //Path.Combine(Directory.GetCurrentDirectory(), "Views", "StaticFiles", "users.json");

                // Verificar se o arquivo existe
                if (System.IO.File.Exists(usersFilePath))
                {
                    // Ler o conteúdo do arquivo
                    string usersJson = System.IO.File.ReadAllText(usersFilePath);

                    // Desserializar o conteúdo em uma lista de usuários
                    List<Agent> users = JsonSerializer.Deserialize<List<Agent>>(usersJson);

                    // Remover os usuários correspondentes ao parâmetro sip ["sip1","sip2","sip3"]
                    users.RemoveAll(u => sips.Contains(u.Sip));

                    // Serializar a lista de usuários atualizada em JSON
                    string usersJsonUpdated = JsonSerializer.Serialize(users);

                    // Gravar a lista de usuários atualizada no arquivo users.json
                    System.IO.File.WriteAllText(usersFilePath, usersJsonUpdated);
                }
                return RestartService();

            }
            catch (Exception ex)
            {
                return new { success = false, message = ex.Message };
            }


        }
    }
}