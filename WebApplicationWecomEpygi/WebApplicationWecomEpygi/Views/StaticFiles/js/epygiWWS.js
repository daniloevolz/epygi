var supporters = []
fetch('https://wetransfer.wecom.com.br:81/StaticFiles/users.json')
 .then(response => response.json())
 .then( data => {

    console.log(data)
    supporters = data
   })
  .catch(error => {
    console.error('Erro ao carregar o arquivo JSON', error);
  });

// variável para armazenar o identificador do intervalo
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
    const ulUsers = document.getElementById("ul-users");
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
        ulUsers.innerHTML = ""; 
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
           // ulUsers.innerHTML = "";
           // updateUsersHTML(department, response); 
          })
          .catch(error => {
            console.error('Erro ao fazer a requisição:', error);
            ulUsers.innerHTML = "";
            updateUsersHTML(department, "");
          });
        
      }, 10000); // 1 minuto = 60 segundos = 60000 milissegundos  
}

/* get no banco -> tabela de departamentos */

function getDepartments(){
   fetch("/Home/Departments", {
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
       'Accept': '*/*',
        'Content-Length': contentLength.toString()
    },
  }).then(response => response.json())
     .then(data => {
      var response = data;
      makeCards(response)
    })
    .catch(error => {
       console.error('Erro ao fazer a requisição:', error);
    });
}
 function buildCardsHTML(department){
      var cards = `
     <li class="bgdBlue"><a href="#" class="card white pre-vendas" id="${department}">
       <h5>${department}</h5>
       <span class="bgWhite titillium dBlue">Conectar</span>
     </a></li>
    `
  return cards
}
 function makeCards(department){
    var divCards = document.getElementById("div-cards");
     const ulCards = document.getElementById("ul-cards");
    department.forEach(function(){
      var cardHTML = buildCardsHTML(department);
     divCards.innerHTML = ''
      ulCards.innerHTML += cardHTML
     })
    
}


document.getElementById("clickBack").addEventListener("click",function(){
    document.getElementById("div-cards").style.display = 'block'
    document.getElementById("div-users").style.display = 'none'
    clearInterval(intervalId);
  })

function buildUserHTML(user, response) {
  var userStatus = response.find(function(item) {
    return item[user.sip] !== undefined;
  });

  var statusClass = userStatus ? userStatus[user.sip] : 'Offline';
  var html = `
  <li>
<div class="user-card">
  <img src="${user.img}" class = "${statusClass}">
  <div class="user-details">
    <div class="user-name">${user.name}</div>
    <div class="user-status">${statusClass}</div>
  </div>
  <div style="display:flex ; justify-content: flex-end ; width: 100%;align-items:center">
    <div onclick="prepareCall('${user.sip}', '${user.num}', '${statusClass}')"> 
    <img src="./images/call.png" class="img-icons" id="imgcall">
    </div>
    <div onclick="location.href='mailto:${user.email}'">
    <img src="./images/mensagem.png" class="img-icons">
    </div>
   
    </div>
    <div style="    
    /* width: 100%; */
    /* position: relative; */
    position: absolute;
    height:100%;
    top: calc(100% - 6px);
    float: right;
    margin-left: 80%;
    width: 20%;
    margin-right: margin;
   ">
    <div class="status-line ${statusClass}"> </div>
    </div>
</div>
</li>
  `;
  return html;
  
}

function updateUsersHTML(department, response) {
    const divUsers = document.getElementById("div-users");
    var divDepart = document.getElementById("depart-div");
    const ulUsers = document.getElementById("ul-users");
    document.getElementById("div-users").style.display = 'block';
    document.getElementById("div-cards").style.display = 'none'

  supporters.forEach(function(supporter) {
    if (supporter.department === department) {
      var userHTML = buildUserHTML(supporter, response);
      divDepart.innerHTML = ''
      divDepart.innerHTML += supporter.department
      ulUsers.innerHTML += userHTML;
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
  const lis = document.querySelectorAll("a.card");

  lis.forEach(li => {
    li.addEventListener("click", function() {
      const id = this.id;
      getUsersStatus(id);
      
    });
  });
  
  