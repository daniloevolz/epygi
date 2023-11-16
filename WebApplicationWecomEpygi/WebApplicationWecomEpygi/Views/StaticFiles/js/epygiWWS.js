var supporters = []
var listStatus = []
var urlEpygi = "https://epygidemo.wecom.com.br/ctc/";
var urlUsers = "https://wetransfer.wecom.com.br:81/Home/UsersPublic";
const urlStatus = 'https://wetransfer.wecom.com.br:9090/api/pabx/prslistrequest';
var urlDepart = "https://wetransfer.wecom.com.br:81/Home/Departments";
var urlStatusColor = "https://wetransfer.wecom.com.br:81/Home/Status";



function load() {
    getDepartments();
    fetch(urlUsers)
        .then(response => response.json())
        .then(data => {

            console.log(data)
            supporters = data
        })
        .catch(error => {
            console.error('Erro ao carregar o arquivo JSON', error);
        });
    fetch(urlStatusColor)
        .then(response => response.json())
        .then(data => {
            listStatus = data
        })
        .catch(error => {
            console.error('Erro ao carregar o arquivo JSON', error);
        });
}

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
    const data = getUsersByDepartment(department);
    const divCards = document.getElementById("div-cards");
    const divUsers = document.getElementById("div-users");
    const ulUsers = document.getElementById("ul-users");
    const divContent = document.getElementById("div-content");

    const requestBody = JSON.stringify(data);
    const contentLength = requestBody.length;
    fetch(urlStatus, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'Accept': '/',
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
    intervalId = setInterval(function () {
        fetch(urlStatus, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': '/',
                'Content-Length': contentLength.toString()
            },
            body: JSON.stringify(data)
        })
            .then(response => response.json())
            .then(jsonData => {
                const response = jsonData;
                ulUsers.innerHTML = "";
                updateUsersHTML(department, response);
            })
            .catch(error => {
                console.error('Erro ao fazer a requisição:', error);
                ulUsers.innerHTML = "";
                updateUsersHTML(department, "");
            });

    }, 10000); // 1 minuto = 60 segundos = 60000 milissegundos  
}


/* get no banco -> tabela de departamentos */

function getDepartments() {
    fetch(urlDepart, {
        method: 'GET'
    }).then(response => response.json())
        .then(data => {
            var response = data;
            makeCards(response)
        })
        .catch(error => {
            console.error('Erro ao fazer a requisição:', error);
        });
}
function buildCardsHTML(department) {
    var cards = `
     <li class="bgdBlue"><a href="#" class="card white pre-vendas" id="${department.Department}">
       <h5>${department.Department}</h5>
       <span class="bgWhite titillium dBlue">Conectar</span>
     </a></li>
    `
    return cards
}
function makeCards(departments) {
    var divCards = document.getElementById("div-cards");
    const ulCards = document.getElementById("ul-cards");
    ulCards.innerHTML = ''
    departments.forEach(function (department) {
        var cardHTML = buildCardsHTML(department);

        ulCards.innerHTML += cardHTML
    })
    const lis = document.querySelectorAll("a.card");

    lis.forEach(li => {
        li.addEventListener("click", function () {
            const id = this.id;
            getUsersStatus(id);

        });
    });

}

document.getElementById("clickBack").addEventListener("click", function () {
    document.getElementById("div-cards").style.display = 'block';
    document.getElementById("div-users").style.display = 'none';
    document.getElementById("div-call").innerHTML = '';
    document.getElementById("div-call").style.display = 'none';
    document.getElementById("div-content").style.display = 'flex';
    clearInterval(intervalId);
})

function buildUserHTML(user, response) {
    var userStatus = response.find(function (item) {
        return item[user.sip] !== undefined;
    });
    var statusClass = userStatus ? userStatus[user.sip] : 'Offline';
    var statusName = 'Offline';
    var statusColor;

    for (let i = 0; i < listStatus.length; i++) {
        if (listStatus[i].Id === statusClass) {
            statusName = listStatus[i].StatusName;
            statusColor = listStatus[i].Color
            break;
        }
    }

    var html;


    html = `
    
        <li  style = "background: ${statusColor}" class = "card-userLi ${statusColor}">
        <div class="availability-user">

      <div class="user-card" >
        <img src="${user.img}" style = "border: 3px solid ${statusColor}" class="${statusColor}">
        <div class="user-details">
          <div class="user-name">${user.name}</div>
          <div class="user-status">${statusName}</div>
        </div>
        
        <div style="display:flex ; justify-content: flex-end ; width: 100%;align-items:center; padding-top: 7px;">
        <div> 
        <img src="./images/video-icon.png" class="img-icons">
        </div>
        <div> 
        <img src="./images/chat-icon.png" class="img-icons">
        </div>
          <div onclick="prepareCall('${user.img}' , '${user.sip}' , '${user.department}', '${user.num}', '${statusClass}')"> 
          <img src="./images/phone-icon.png" class="img-icons" id="imgcall">
          </div>
          <div onclick="location.href='mailto:${user.email}'">
          <img src="./images/mail-icon.png" class="img-icons">
          </div>
          </div>
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
    document.getElementById("div-cards").style.display = 'none';
    divDepart.innerHTML = '';
    divDepart.innerHTML = department;

    supporters.forEach(function (supporter) {
        if (supporter.department === department) {
            var userHTML = buildUserHTML(supporter, response);
            ulUsers.innerHTML += userHTML;
        }
    })
}

var btnDeclineListener = null;

function prepareCall(img ,id, depart, num, status) {
    if (status == "online") {
        
        var divCall = document.getElementById("div-call");
        var divContent = document.getElementById("div-content");
        // var divIframe = document.createElement("div")
        // divIframe.setAttribute("id", "diviframe") 
        // divIframe.setAttribute("class", "diviframe") 
        // divIframe.style.width = '40%'
        divCall.innerHTML = ''
        divContent.style.display = 'block';
        // divIframe.style.display = 'none';
        divCall.style.display = 'flex';
        var iframe = document.createElement("iframe");
        iframe.setAttribute("src",urlEpygi + id)
        iframe.setAttribute("id","iframe-call")
        iframe.setAttribute("allow","camera;microphone")
        // iframe.style.width = "100%";
        // iframe.style.paddingLeft = "10px";
        iframe.style.height = "100%";
        iframe.style.visibility = 'visible';
        iframe.style.display = 'none';
        // iframe.style.position = 'absolute';
        // iframe.style.left = '5%'

        var DivCallStatus = `
        <div class = "main-call" >
        <div class="card-call" id="card-call">
        <div class="header-call">
        <div class="img-on-call"><img src="${img}" width = 135px  style="border-radius: 7px;"></div>
        </div>
        <div class="outgoing-name">
            <div>${id}</div>
            <span>${depart}</span>
        </div>
        <div class="icons-call">
        <img src="./images/hang-up-icon.png" width="35px" id="DeclineCall" style="margin: 7px; margin-right: 20px; cursor: pointer">
        <img src="./images/keyboard-icon.png" width="35px" id="keyboard" style="margin: 7px; cursor: pointer ">
       </div>
        </div>
        <div id = "iframe-div">
        </div>
        </div>
          `
        // divIframe.appendChild(iframe)
        divCall.innerHTML += DivCallStatus
        document.getElementById("iframe-div").appendChild(iframe)
        // divCall.appendChild(divIframe);
        
        if (btnDeclineListener) {
            document.getElementById("DeclineCall").removeEventListener("click", btnDeclineListener);
        }else{
            btnDeclineListener = function() {
                divCall.innerHTML = '';
                divCall.style.display = 'none';
                divContent.style.display = 'flex';
            };
        }
        document.getElementById("keyboard").addEventListener("click", btnKeyboard);
        document.getElementById("DeclineCall").addEventListener("click", btnDeclineListener);

        function btnKeyboard() {

            var iframe = document.getElementById("iframe-call");
            if (iframe.style.display == 'block') {
                /*document.getElementById("keyboard").removeEventListener("click", btnDeclineListener);*/
                iframe.style.display = 'none';
                document.getElementById("card-call").style.right = '100px'
            }
            else {
                iframe.style.display = 'block';
            }

        }
        
    } else {
        window.alert("Usuário indisponível no momento!");
    }
}
