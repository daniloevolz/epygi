
var cookie;
var cookieName = "successLoginCookie";
var supporters = [];
var intervalId;

//   function load() {
//     // exemplo de uso: obtém o valor do cookie "successCookie"
//      var successValue = getCookie(cookieName);
//     if (successValue == null) {
//         window.location.href = "./login.html";
//      } else {
//        cookie = successValue;
//      }
//      showHome();
//  }

// função para obter o valor de um cookie pelo nome
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');

    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];

        while (c.charAt(0) == ' ') {
            c = c.substring(1, c.length);
        }

        if (c.indexOf(nameEQ) == 0) {
            var cookieValue = c.substring(nameEQ.length, c.length);

            // Obter a data de expiração do cookie
            var cookieExpiration = getCookieExpiration(cookieValue);

            // Verificar se a data de expiração é anterior à data e hora atual
            if (cookieExpiration && cookieExpiration < new Date()) {
                // Remover o cookie definindo uma data de expiração no passado
                document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";

                return null;
            }

            return cookieValue;
        }
    }

    return null;
}

// Função auxiliar para obter a data de expiração do cookie
function getCookieExpiration(cookieValue) {
    var cookieParts = cookieValue.split(';');

    for (var i = 0; i < cookieParts.length; i++) {
        var cookiePart = cookieParts[i].trim();

        if (cookiePart.indexOf('expires=') === 0) {
            var expirationString = cookiePart.substring('expires='.length);
            return new Date(expirationString);
        }
    }

    return null;
}
function deleteCookie() {
    document.cookie = cookieName + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";
}
// requisição post para adicionar usuarios
document.getElementById('a-upload-user').addEventListener('click', function (e) {
    e.preventDefault();

    const name = document.getElementById('name').value;
    const sip = document.getElementById('sip').value;
    const num = document.getElementById('num').value;
    const pass = document.getElementById('pass').value;
    const department = document.getElementById('departamento').value.toLowerCase();
    const location = document.getElementById('location').value;
    const email = document.getElementById('email').value;
    const imgFile = document.getElementById('file-upload-button').files[0]; // obter o arquivo de imagem

    
    if (name === '' || sip === '' || num === '' || department === '' || location === '' || email === '' || !imgFile) {
        // makePopUp();
        showToast("warning","Complete todos os campos")
        return; 
    }

    const data = {
        "user": {
            "name": name,
            "sip": sip,
            "num": num,
            "pass": pass,
            "department": department,
            "location": location,
            "email": email
        },
        "image": {
            "name": imgFile.name,
            "size": imgFile.size,
            "data": null // será preenchido posteriormente
        }
    };

    const reader = new FileReader();
    reader.onload = function (event) {
        data.image.data = event.target.result; // definir o conteúdo da imagem no objeto de dados
        // enviar os dados para o servidor
    
        fetch('/Home/AddUser', {
            method: 'POST',
            headers: {
                'Content-Type' : 'application/json',
                'Authorization': "Bearer " + cookie
            },
            body: JSON.stringify(data)
        })
            .then(async function (response) {
                if (response.ok) {
                    const data = await response.json();
                    if (data.success == true) {
                        showToast("success", data.message)
                        showHome();

                    } else {
                        showToast("warning", data.message)
                    }
                    
                } else {
                    console.log('Ocorreu um erro ao salvar os dados.');
                    showToast("danger",response.status)
                }
            })
            .catch(error => {
                console.error('Erro:', error);
                showToast("danger", error)
            });
    
    };
    reader.readAsDataURL(imgFile); // ler o conteúdo da imagem como base64

});

function getUsersByDepartment(department) {
  var users = [];

  if(department == "all"){
    for (var i = 0; i < supporters.length; i++) {
        users.push(supporters[i].sip);
    }
  }else{
    for (var i = 0; i < supporters.length; i++) {
      if (supporters[i].department === department) {
        users.push(supporters[i].sip);
      }
    }
  }
  return { users: users };
}
function getUsersStatus(department) {
    const url = 'https://wetransfer.wecom.com.br:9090/api/pabx/prslistrequest'; // Substitua pela URL real...
    const data = getUsersByDepartment(department);
    const divCards = document.getElementById("main-cards");

  const requestBody = JSON.stringify(data);
  const contentLength = requestBody.length;
    fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
          'Accept': '*/*',
          'Content-Length': contentLength.toString()
      },
      body: JSON.stringify(data)
    }).then(response => response.json()).then(jsonData => {
        const response = jsonData;
        divCards.innerHTML = ""; 
        updateUsersHTML(department, response);
      })
      .catch(error => {
        console.error('Erro ao fazer a requisição:', error);
        updateUsersHTML(department, "");
      });
      clearInterval(intervalId); // parar interval
      intervalId = setInterval(function() {
        fetch(url, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
              'Accept': '*/*',
              'Content-Length': contentLength.toString()
          },
          body: JSON.stringify(data)
        })
          .then(response => response.json())
          .then(jsonData => {
            const response = jsonData;
            divCards.innerHTML = "";
            updateUsersHTML(department, response); 
          })
          .catch(error => {
            console.error('Erro ao fazer a requisição:', error);
            divCards.innerHTML = "";
            updateUsersHTML(department, "");
          });
        
      }, 10000); // 1 minuto = 60 segundos = 60000 milissegundos  
}
  // Função para construir a estrutura HTML com base nos valores correspondentes
function buildUserHTML(user, response) {
  var userStatus = response.find(function(item) {
    return item[user.sip] !== undefined;
  });

  var statusClass = userStatus ? userStatus[user.sip] : 'Offline';
  var html = `
  <div class="cards" id="cards">
  <div class="epygi-root-visitenkarten" style="top: -10px; font-size: 10px; left: 5px; background-color: transparent; width: 240px; margin: 0;">
    <div class="epygi-image">
      <img src=${user.img} class="epygi-tab__supporter-img" alt="">
    </div>
    <div class="epygi-content" style="width: 200px; height: 45px;margin-top: -6%;">
      <div class="epygi-content__headline" style="text-transform: capitalize">
        <strong>${user.name}<br></strong>${user.department}<br>
      </div>
      <div class="epygi-content__status" style="display:flex;align-items:center;">
        <div class="epygi-content__status__indicator ${statusClass} "></div>
        <div>${statusClass}</div>
      </div>    
    </div>
  </div>
</div>
  `;
  return html;
  
}
// Função para atualizar a div 'div-users' com a estrutura HTML construída
function updateUsersHTML(department, response) {

  var divCards = document.getElementById("main-cards")

  supporters.forEach(function(supporter) {
    if (supporter.department === department || department == "all") {
      var userHTML = buildUserHTML(supporter, response);
        // divCards.innerHTML =''
        divCards.innerHTML += userHTML
    }
  });

}
function showToast(type, message) {
  // Cria o elemento de mensagem toast com base no tipo fornecido
  var toastElement = document.createElement('div');
  toastElement.classList.add(type, 'alert');

  // Cria o conteúdo da mensagem toast
  var contentElement = document.createElement('div');
  contentElement.classList.add('content');

  // Verifica o tipo fornecido e define o ícone apropriado
  var iconElement = document.createElement('div');
  iconElement.classList.add('icon');
  var iconSvg;
  switch (type) {
    case 'success':
      iconSvg = '<svg width="50" height="50" id="Layer_1" style="enable-background:new 0 0 128 128;" version="1.1" viewBox="0 0 128 128" xml:space="preserve" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink"><g><circle fill="#fff" cx="64" cy="64" r="64"/></g><g><path fill="#3EBD61" d="M54.3,97.2L24.8,67.7c-0.4-0.4-0.4-1,0-1.4l8.5-8.5c0.4-0.4,1-0.4,1.4,0L55,78.1l38.2-38.2   c0.4-0.4,1-0.4,1.4,0l8.5,8.5c0.4,0.4,0.4,1,0,1.4L55.7,97.2C55.3,97.6,54.7,97.6,54.3,97.2z"/></g></svg>';
      break;
    case 'info':
      iconSvg = '<svg width="50" height="50" viewBox="0 0 50 50" fill="none" xmlns="http://www.w3.org/2000/svg"><rect width="50" height="50" rx="25" fill="white"/><path d="M27 22H23V40H27V22Z" fill="#006CE3"/><path d="M25 18C24.2089 18 23.4355 17.7654 22.7777 17.3259C22.1199 16.8864 21.6072 16.2616 21.3045 15.5307C21.0017 14.7998 20.9225 13.9956 21.0769 13.2196C21.2312 12.4437 21.6122 11.731 22.1716 11.1716C22.731 10.6122 23.4437 10.2312 24.2196 10.0769C24.9956 9.92252 25.7998 10.0017 26.5307 10.3045C27.2616 10.6072 27.8864 11.1199 28.3259 11.7777C28.7654 12.4355 29 13.2089 29 14C29 15.0609 28.5786 16.0783 27.8284 16.8284C27.0783 17.5786 26.0609 18 25 18V18Z" fill="#006CE3"/></svg>';
      break;
    case 'warning':
      iconSvg = '<svg height="50" viewBox="0 0 512 512" width="50" xmlns="http://www.w3.org/2000/svg"><path fill="#fff" d="M449.07,399.08,278.64,82.58c-12.08-22.44-44.26-22.44-56.35,0L51.87,399.08A32,32,0,0,0,80,446.25H420.89A32,32,0,0,0,449.07,399.08Zm-198.6-1.83a20,20,0,1,1,20-20A20,20,0,0,1,250.47,397.25ZM272.19,196.1l-5.74,122a16,16,0,0,1-32,0l-5.74-121.95v0a21.73,21.73,0,0,1,21.5-22.69h.21a21.74,21.74,0,0,1,21.73,22.7Z"/></svg>';
      break;
    case 'danger':
      iconSvg = '<svg height="50" viewBox="0 0 512 512" width="50" xmlns="http://www.w3.org/2000/svg"><path fill="#fff" d="M449.07,399.08,278.64,82.58c-12.08-22.44-44.26-22.44-56.35,0L51.87,399.08A32,32,0,0,0,80,446.25H420.89A32,32,0,0,0,449.07,399.08Zm-198.6-1.83a20,20,0,1,1,20-20A20,20,0,0,1,250.47,397.25ZM272.19,196.1l-5.74,122a16,16,0,0,1-32,0l-5.74-121.95v0a21.73,21.73,0,0,1,21.5-22.69h.21a21.74,21.74,0,0,1,21.73,22.7Z"/></svg>';
      break;
    default:
      break;
  }
  iconElement.innerHTML = iconSvg;

  // Cria o parágrafo da mensagem
  var messageElement = document.createElement('p');
  messageElement.textContent = message;

  // Cria o botão de fechar
  var closeElement = document.createElement('button');
  closeElement.classList.add('close');
  closeElement.innerHTML = '<svg height="18px" id="Layer_1" style="enable-background:new 0 0 512 512;" version="1.1" viewBox="0 0 512 512" width="18px" xml:space="preserve" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/svg"><path fill="#69727D" d="M437.5,386.6L306.9,256l130.6-130.6c14.1-14.1,14.1-36.8,0-50.9c-14.1-14.1-36.8-14.1-50.9,0L256,205.1L125.4,74.5  c-14.1-14.1-36.8-14.1-50.9,0c-14.1,14.1-14.1,36.8,0,50.9L205.1,256L74.5,386.6c-14.1,14.1-14.1,36.8,0,50.9  c14.1,14.1,36.8,14.1,50.9,0L256,306.9l130.6,130.6c14.1,14.1,36.8,14.1,50.9,0C451.5,423.4,451.5,400.6,437.5,386.6z"/></svg>';

  // Adiciona os elementos criados à mensagem toast
  contentElement.appendChild(iconElement);
  contentElement.appendChild(messageElement);
  toastElement.appendChild(contentElement);
  toastElement.appendChild(closeElement);

  // Adiciona a mensagem toast à div "container" existente na página
  var containerElement = document.querySelector('.colRight');
  containerElement.appendChild(toastElement);

  // Define o tempo para a mensagem toast desaparecer (opcional)
  setTimeout(function () {
    toastElement.remove();
  }, 3000); // A mensagem toast será removida após 3 segundos (3000 milissegundos)
}
  // evt listeners para opções menu lateral
  document.getElementById("dashhome").addEventListener("click", function(){
        console.log("click Dash Home")
        showHome();
  });
  document.getElementById("useradd").addEventListener("click", function(){
    console.log("click Adição de Usuário")
    document.getElementById("id-home").style.display = "none"
    document.getElementById("id-add-home").style.display = "flex"
    document.getElementById("id-add-dep").style.display = "none"
    document.getElementById("id-list-home").style.display = "none"
  });
  document.getElementById("departadd").addEventListener("click", function(){
    console.log("click Adição de Departamento")
    document.getElementById("id-home").style.display = "none"
    document.getElementById("id-add-home").style.display = "none"
    document.getElementById("id-add-dep").style.display = "flex"
    document.getElementById("id-list-home").style.display = "none"
  });
   document.getElementById("userlist").addEventListener("click", function(){
    console.log("click Lista de Usuário")
      showListUsers();
  });
document.getElementById("logout").addEventListener("click", function () {
    console.log("click Sair")
    deleteCookie();
    window.location.href = "./login.html";
});
  document.getElementById("logo-box").addEventListener("click", function(){
  
  });
// evt listener para light / dark mode
const themeToggle = document.getElementById('theme');
const themeLink = document.getElementById('theme-dark');

themeToggle.addEventListener('click', () => {
  if (themeLink.getAttribute('href') === '../css/admin-interf.css') {
    
    themeLink.setAttribute('href', '../css/themewhite-adm-interf.css');
    document.getElementById("sun-moon").setAttribute("src", "../images/moon.png")
  } else {
    themeLink.setAttribute('href', '../css/admin-interf.css');
    document.getElementById("sun-moon").setAttribute("src", "../images/sunny-day.png")
  }
});

function showHome() {
    document.getElementById("id-home").style.display = "block"
    document.getElementById("id-add-home").style.display = "none"
    document.getElementById("id-add-dep").style.display = "none"
    document.getElementById("id-add-local").style.display = "none"
    document.getElementById("id-list-home").style.display = "none"
    fetch('users.json')
        .then(response => response.json())
        .then(data => {
            supporters = data;
            const departmentSelect = document.getElementById("filter-department");
            const uniqueDepartments = new Set();

            // Adiciona o item "Todos" no início da lista de departamentos
            const optionTodos = document.createElement("option");
            optionTodos.value = "todos";
            optionTodos.text = "todos";
            optionTodos.id = "all";
            departmentSelect.appendChild(optionTodos);

            data.forEach(obj => {
                const { department } = obj;
                uniqueDepartments.add(department);
            });

            const sortedDepartments = Array.from(uniqueDepartments).sort();
            sortedDepartments.forEach(department => {
                const option = document.createElement("option");
                option.value = department;
                option.text = department;
                option.id = department
                departmentSelect.appendChild(option);
            });
            getUsersStatus("all")
            departmentSelect.addEventListener("change", function (event) {
                const selectedOption = event.target.selectedOptions[0];
                const selectedDepartmentId = selectedOption.id;
                getUsersStatus(selectedDepartmentId);
            })
        })
}
function showListUsers() {
    document.getElementById("id-home").style.display = "none"
    document.getElementById("id-add-home").style.display = "none"
    document.getElementById("id-add-dep").style.display = "none"
    document.getElementById("id-add-local").style.display = "none"
    document.getElementById("id-list-home").style.display = "block"
    document.getElementById("data-table").innerHTML = '';
    // tabela de usuários adm
    var users = []
    var titletable = `<th></th>
        <th>Nome</th>
        <th>Departamento</th>
        <th>SIP</th>
        <th>E-mail</th>
        <th>Ramal</th>
        <th>Local</th>`;
    document.getElementById("data-table").innerHTML += titletable;
    fetch('./users.json')
        .then(response => response.json())
        .then(data => {
            users = data
            makeUserTable(users);
            addDeleteEventListeners();
        })
        .catch(error => {
            console.error('Erro ao carregar o arquivo JSON', error);
        });
}
function makeUserTable(users) {
  users.forEach(function (user) {
    var html = `
      <tr>
        <td><img src="${user.img}" alt="" style="width: 35px; height: 35px;"></td>
        <td>${user.name}</td>
        <td style="text-transform: capitalize; text-align: center;">${user.department}</td>
        <td style="text-align: center;"><strong>${user.sip}</strong></td>
        <td>${user.email}</td>
        <td style="text-transform: capitalize; text-align: center;">${user.num}</td>
        <td style="text-transform: capitalize; text-align: center;">${user.location}</td>
        <td style="text-align: center;"><img class="delete-user" id="${user.sip}" src="./images/trash.png" alt="" style="width: 25px; height: 25px;"></td>
      </tr>
    `;
    document.getElementById("data-table").innerHTML += html
  });
}
function addDeleteEventListeners() {
  var deleteButtons = document.getElementsByClassName("delete-user");
  Array.from(deleteButtons).forEach(function (button) {
    button.addEventListener("click", deleteUser);
  });
}

function deleteUser(event) {
  var userSIP = event.target.id;

  fetch("/Home/DelUser", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      'Authorization': "Bearer " + cookie
    },
    body: JSON.stringify({ SIP: userSIP }),
  })
    .then(response => response.json())
    .then(data => {
      console.log("usuário removido com sucesso:", data);
      removeUserRow(userSIP);
    })
    .catch(error => {
      console.error("Erro ao remover usuário:", error);
    });
}
// parent Node para pegar o TR pai 
function removeUserRow(userSIP) {
  var tableRow = document.querySelector(`#data-table tr td img[id="${userSIP}"]`).parentNode.parentNode;
  if (tableRow) {
    tableRow.remove();
  }
}






