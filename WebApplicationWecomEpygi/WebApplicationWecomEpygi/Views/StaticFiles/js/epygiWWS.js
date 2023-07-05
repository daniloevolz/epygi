var supporters = []
fetch('./users.json')
 .then(response => response.json())
 .then( data => {

    console.log(data)
    supporters = data

    // const dadosDiv = document.getElementById('dataUsers');
    // dadosDiv.innerText = JSON.stringify(dataUsers);
   })
  .catch(error => {
    console.error('Erro ao carregar o arquivo JSON', error);
  });

// Variável para armazenar o identificador do intervalo
var intervalId;
function getUsersByDepartment(department) {
  var users = [];

  for (var i = 0; i < supporters.length; i++) {
    if (supporters[i].department === department) {
      users.push(supporters[i].sip);
    }
  }

  return { users: users };
}

function getUsersStatus(department) {
    const url = 'https://wetransfer.wecom.com.br:9090/api/pabx/prslistrequest'; // Substitua pela URL real...
    const data = getUsersByDepartment(department);
    const divCards = document.getElementById("div-cards");
    const divUsers = document.getElementById("div-users");
    const divContent = document.getElementById("div-content");

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
        updateUsersHTML(department, response);
      })
      .catch(error => {
        console.error('Erro ao fazer a requisição:', error);
        updateUsersHTML(department, "");
      });
      clearInterval(intervalId);
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
            divUsers.innerHTML = "";
            updateUsersHTML(department, response);
          })
          .catch(error => {
            console.error('Erro ao fazer a requisição:', error);
            divUsers.innerHTML = "";
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
  <ul>
  <li>
<div class="user-card">
  <img src="${user.img}" class = "${statusClass}">
  <div class="user-details">
    <div class="user-name">${user.name}</div>
    <div class="user-status">${statusClass}</div>
  </div>
 <div class="user-icons">
    <img src="./images/call.png" class="img-icons">
    <img src="./images/mensagem.png" class="img-icons">
  </div>
  
</div>
</li>
</ul>
  `;

  return html;
}

// Função para atualizar a div 'div-users' com a estrutura HTML construída
function updateUsersHTML(department, response) {
    const divUsers = document.getElementById("div-users");
    document.getElementById("div-users").style.display = 'block';
    document.getElementById("div-cards").style.display = 'none'

  supporters.forEach(function(supporter) {
    if (supporter.department === department) {
      var userHTML = buildUserHTML(supporter, response);
      divUsers.innerHTML += userHTML;
    }
  });

}
  function prepareCall(id, num, status){
    if(status=="online"){
      const divContent = document.getElementById("div-content");
      var url ="https://epygidemo.wecom.com.br/ctc/";
      // Limpar o conteúdo existente da div-content
      //divContent.innerHTML = "";

      // Criar um iframe
      const iframe = document.createElement("iframe");
      iframe.style.width = "100%";
      iframe.style.height = "100%";
      iframe.src = url+id; // Substitua pela URL desejada

      // Adicionar o iframe à div-content
      //divContent.appendChild(iframe);
      // Obter as dimensões da janela do navegador
      const screenWidth = window.innerWidth || document.documentElement.clientWidth || document.body.clientWidth;
      const screenHeight = window.innerHeight || document.documentElement.clientHeight || document.body.clientHeight;

      // Calcular as coordenadas para posicionar a janela no centro
      const windowWidth = 200; // Largura da janela
      const windowHeight = 400; // Altura da janela
      const left = (screenWidth - windowWidth) / 2;
      const top = (screenHeight - windowHeight) / 2;

      // Abrir o conteúdo desejado em uma nova janela ou guia, posicionada no centro da tela
      window.open(url+id, "_blank", "toolbar=no,width="+windowWidth+",height="+windowHeight+",left=" + left + ",top=" + top);
    }else{
      window.alert("Usuário indisponível no momento!");
    }
      

  }
  // Obtendo todos os elementos <li> com a classe "bgdBlue"
  const lis = document.querySelectorAll("a.card");

  // Adicionando um ouvinte de eventos de clique a cada elemento <li>
  lis.forEach(li => {
    li.addEventListener("click", function() {
      const id = this.id;
      getUsersStatus(id);
      

    });
  });
  
  