function getUsersStatus(sector) {
    const url = 'http://127.0.0.1:999/api/pabx/prslistrequest'; // Substitua pela URL real
    const data = { users: ['Erick', 'Danilo', 'Pietro', 'Daniel'] };
  
    fetch(url, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(data)
    })
      .then(response => response.json())
      .then(jsonData => {
        const users = jsonData;
        
        for (const username in users) {
          const status = users[username];
          console.log(`${username}: ${status}`);

        }
        updateUserStatus(users);
      })
      .catch(error => {
        console.error('Erro ao fazer a requisição:', error);
      });
  }
  function updateUserStatus(users){



  };
  function prepareCall(id, num, video){
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

  }
  // Obtendo todos os elementos <li> com a classe "bgdBlue"
const lis = document.querySelectorAll("a.card");

// Adicionando um ouvinte de eventos de clique a cada elemento <li>
lis.forEach(li => {
  li.addEventListener("click", function() {
    const id = this.id;
    getUsersStatus(id);
    // Realizar as alterações na estrutura da página
    const sectionPrincipal = document.getElementById("sectionPrincipal");
    const divCards = document.getElementById("div-cards");
    const divUsers = document.getElementById("div-users");
    const divContent = document.getElementById("div-content");

    // Definir os novos estilos para as divs
    divCards.style.width = "40%";
    divUsers.style.width = "40%";
    divUsers.style.display = "block";
    divContent.style.width = "20%";


  });
});
  
  