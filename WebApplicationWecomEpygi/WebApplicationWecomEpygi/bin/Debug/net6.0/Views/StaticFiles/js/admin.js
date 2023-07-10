
var cookie;
var cookieName = "successLoginCookie";
 
 function load() {
    // exemplo de uso: obtém o valor do cookie "successCookie"
     var successValue = getCookie(cookieName);
    if (successValue == null) {
        window.location.href = "./login.html";
    } else {
       cookie = successValue;
    }
}
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
            .then(response => {
                if (response.ok) {
                    console.log('Dados salvos com sucesso!');
                } else {
                    console.log('Ocorreu um erro ao salvar os dados.');
                }
            })
            .catch(error => {
                console.error('Erro:', error);
            });
    
    };
    reader.readAsDataURL(imgFile); // ler o conteúdo da imagem como base64

    // showUsersDiv();
    // buildUserHTML(users)

});


// função para obter o valor de um cookie pelo nome
// function makePopUp(){
//     var error = document.getElementById("loginErro");
//     error.style.display = 'block';
//     setTimeout(function(){
//         error.style.display = 'none'
//     },1200)

// }


var supporters = [];

// filtro de usuários 

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
      departmentSelect.addEventListener("change", function(event) {
      const selectedOption = event.target.selectedOptions[0];
      const selectedDepartmentId = selectedOption.id;
      getUsersStatus(selectedDepartmentId);
  })
  })
  //   users = data;
  //   displayUsers(users); // Exibe todos os usuários inicialmente

  //   departmentSelect.addEventListener("change", function() {
  //     const selectedDepartment = departmentSelect.value;
  //     if(selectedDepartment === "todos"){
  //       displayUsers(users)
  //     }else{
  //     const filteredUsers = users.filter(user => user.department === selectedDepartment);
  //     displayUsers(filteredUsers); // Exibe apenas os usuários filtrados
  //   }
  //   });
  // })
  // .catch(error => {
  //   console.log(error);
  // });

  // function displayUsers(users) {
  //   const departmentSelect = document.getElementById("filter-department");
  //   const selectedDepartment = departmentSelect.value;
  
  //   let filteredUsers = users;
  //   if (selectedDepartment !== "todos" ) {
  //     filteredUsers = users.filter(user => user.department === selectedDepartment);
  //   }
  
  //   const html = filteredUsers
  //     .map(user => `
  //       <div class="cards" id="cards">
  //         <div class="epygi-root-visitenkarten" style="top: -10px; font-size: 10px; left: 5px; background-color: transparent; width: 240px; margin: 0;">
  //           <div class="epygi-image">
  //             <img src=${user.img} class="epygi-tab__supporter-img" alt="">
  //           </div>
  //           <div class="epygi-content" style="width: 200px; height: 45px;margin-top: -6%;">
  //             <div class="epygi-content__headline" style="text-transform: capitalize">
  //               <strong>${user.name}<br></strong>${user.department}<br>
  //             </div>
  //             <div class="epygi-content__status" style="display:flex;align-items:center;">
  //               <div class="epygi-content__status__indicator "></div>
  //               <div>Online</div>
  //             </div>    
  //           </div>
  //         </div>
  //       </div>
  //     `)
  //     .join('');
  
  //   document.getElementById("main-cards").innerHTML = html;
  // }

  // Variável para armazenar o identificador do intervalo
var intervalId;

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

  // evt listeners para opções menu lateral
  document.getElementById("dashhome").addEventListener("click", function(){
    console.log("click Dash Home")
    document.getElementById("id-home").style.display = "block"
    document.getElementById("id-add-home").style.display = "none"
    document.getElementById("id-list-home").style.display = "none"
  });
  document.getElementById("useradd").addEventListener("click", function(){
    console.log("click Adição de Usuário")
    document.getElementById("id-home").style.display = "none"
    document.getElementById("id-add-home").style.display = "flex"
    document.getElementById("id-list-home").style.display = "none"
  });
  document.getElementById("userlist").addEventListener("click", function(){
    console.log("click Lista de Usuário")
    document.getElementById("id-home").style.display = "none"
    document.getElementById("id-add-home").style.display = "none"
    document.getElementById("id-list-home").style.display = "block"
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





