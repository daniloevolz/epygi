using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebApplicationWecomEpygi.Models;

namespace WebApplicationWecomEpygi.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public IActionResult AddUser([FromBody] JsonElement data)
        {
            try
            {
                // Extrair os dados do JSON
                string name = data.GetProperty("user").GetProperty("name").GetString();
                string sip = data.GetProperty("user").GetProperty("sip").GetString();
                string num = data.GetProperty("user").GetProperty("num").GetString();
                string email = data.GetProperty("user").GetProperty("email").GetString();
                string location = data.GetProperty("user").GetProperty("location").GetString();
                string department = data.GetProperty("user").GetProperty("department").GetString();
                string imageName = "/images/" + sip + "-user.jpg";
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

                    }catch(Exception ex)
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
                    img = imageName
                };

                // Adicionar o novo usuário à lista
                users.Add(newUser);

                // Serializar a lista de usuários em JSON
                string usersJsonUpdated = JsonSerializer.Serialize(users);

                // Gravar a lista de usuários atualizada no arquivo users.json
                System.IO.File.WriteAllText(usersFilePath, usersJsonUpdated);

                // Salvar a imagem em /StaticFiles/images/
                string imagesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "images");
                string imagePath = Path.Combine(imagesDirectory, imageName);
                string base64String = imageData.Split(',')[1]; // Remove o prefixo "data:image/jpeg;base64,"
                byte[] imageBytes = Convert.FromBase64String(base64String);
                System.IO.File.WriteAllBytes(imagePath, imageBytes);

                return Json(new { success = true, message = "Usuário adicionado com sucesso." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao adicionar usuário: " + ex.Message });
            }
        }
        [HttpPost]
        public IActionResult DelUser([FromBody] List<string> sips)
        {
            try
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

                return Json(new { success = true, message = "Usuários removidos com sucesso." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Erro ao remover usuários: " + ex.Message });
            }
        }

    }
}
