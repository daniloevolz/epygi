
 var cookie;
 /*
 function load() {
    // exemplo de uso: obtém o valor do cookie "successCookie"
     var successValue = getCookie("successLoginCookie");
    if (successValue == null) {
        window.location.href = "./login.html";
    } else {
       cookie = successValue;
    }
 }
 */

// requisição post para adicionar usuarios
document.getElementById('a-upload-user').addEventListener('click', function (e) {
    e.preventDefault();

    const name = document.getElementById('name').value;
    const sip = document.getElementById('sip').value;
    const num = document.getElementById('num').value;
    const department = document.getElementById('department').value.toLowerCase();
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
function getCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
// function makePopUp(){
//     var error = document.getElementById("loginErro");
//     error.style.display = 'block';
//     setTimeout(function(){
//         error.style.display = 'none'
//     },1200)

// }
var users = [];

fetch('users.json')
  .then(response => response.json())
  .then(data => {
    const departmentSelect = document.getElementById("filter-department");
    const uniqueDepartments = new Set();
    
    // Adiciona o item "Todos" no início da lista de departamentos
    const optionTodos = document.createElement("option");
    optionTodos.value = "todos";
    optionTodos.text = "todos";
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
      departmentSelect.appendChild(option);
    });

    users = data;
    displayUsers(users); // Exibe todos os usuários inicialmente

    departmentSelect.addEventListener("change", function() {
      const selectedDepartment = departmentSelect.value;
      if(selectedDepartment === "todos"){
        displayUsers(users)
      }else{
      const filteredUsers = users.filter(user => user.department === selectedDepartment);
      displayUsers(filteredUsers); // Exibe apenas os usuários filtrados
    }
    });
  })
  .catch(error => {
    console.log(error);
  });

  function displayUsers(users) {
    const departmentSelect = document.getElementById("filter-department");
    const selectedDepartment = departmentSelect.value;
  
    let filteredUsers = users;
    if (selectedDepartment !== "todos" ) {
      filteredUsers = users.filter(user => user.department === selectedDepartment);
    }
  
    const html = filteredUsers
      .map(user => `
        <div class="cards" id="cards">
          <div class="epygi-root-visitenkarten" style="top: -10px; font-size: 10px; left: 5px; background-color: transparent; width: 240px; margin: 0;">
            <div class="epygi-image">
              <img src=${user.img} class="epygi-tab__supporter-img" alt="">
            </div>
            <div class="epygi-content" style="width: 200px; height: 45px;margin-top: -6%;">
              <div class="epygi-content__headline" style="text-transform: capitalize">
                <strong>${user.name}<br></strong>${user.department}<br>
              </div>
              <div class="epygi-content__status">
                <div class="epygi-content__status__indicator "></div>
              </div>    
            </div>
          </div>
        </div>
      `)
      .join('');
  
    document.getElementById("main-cards").innerHTML = html;
  }
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
  document.getElementById("logo-box").addEventListener("click", function(){
  
  });

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





