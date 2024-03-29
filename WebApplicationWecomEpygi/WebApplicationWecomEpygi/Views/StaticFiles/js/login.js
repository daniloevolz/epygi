var cookie;
 
//  function load() {
//     // exemplo de uso: obtém o valor do cookie "successCookie"
//      var successValue = getCookie("successLoginCookie");
//     if (successValue != null) {
//         window.location.href = "./admin-interf.html";
//     } else {
//        cookie = successValue;
//     }
// }
// função para obter o valor de um cookie pelo nome
//function getCookie(name) {
//    var nameEQ = name + "=";
//    var ca = document.cookie.split(';');

//    for (var i = 0; i < ca.length; i++) {
//        var c = ca[i];

//        while (c.charAt(0) == ' ') {
//            c = c.substring(1, c.length);
//        }

//        if (c.indexOf(nameEQ) == 0) {
//            var cookieValue = c.substring(nameEQ.length, c.length);

//            // Obter a data de expiração do cookie
//            var cookieExpiration = getCookieExpiration(cookieValue);

//            // Verificar se a data de expiração é anterior à data e hora atual
//            if (cookieExpiration && cookieExpiration < new Date()) {
//                // Remover o cookie definindo uma data de expiração no passado
//                document.cookie = name + "=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;";

//                return null;
//            }

//            return cookieValue;
//        }
//    }

//    return null;
//}



// Função auxiliar para obter a data de expiração do cookie
//function getCookieExpiration(cookieValue) {
//    var cookieParts = cookieValue.split(';');

//    for (var i = 0; i < cookieParts.length; i++) {
//        var cookiePart = cookieParts[i].trim();

//        if (cookiePart.indexOf('expires=') === 0) {
//            var expirationString = cookiePart.substring('expires='.length);
//            return new Date(expirationString);
//        }
//    }

//    return null;
//}

document.getElementById("clickSend").addEventListener("click", function (event) {
    event.preventDefault(); // Evita o comportamento padr�o do formul�rio de recarregar a p�gina

    var username = document.getElementById("name").value;
    var password = document.getElementById("pass").value;

    // Hash MD5 do usu�rio e senha
    var hashedPassword = CryptoJS.MD5(password).toString();

    // Cria um objeto com os dados do login
    var loginData = {
        username: username,
        passwordhash: hashedPassword
    };

    // Envia uma requisi��o POST para o backend
    fetch("/Home/Login", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(loginData)
    }).then(async function (response) {
            if (response.ok) {
                const data = await response.json();
                // Armazena o valor em um cookie chamado "sucessCookie" com uma validade de 30 dias
                setCookie("successLoginCookie", data.success);
                // Login bem-sucedido, redireciona para a p�gina autenticada
                window.location.href = "./admin-interf.html";
            } else {
                    // Login inválido, exibe uma mensagem de erro
                    //makePopUp();  
                    showToast("warning","Erro no Login")
                }
            })
        .catch(function (error) {
            console.error("Erro ao fazer login:", error);
        });
});

document.getElementById("forgotPassword").addEventListener("click", function (event) {
    event.preventDefault(); // Evita o comportamento padr�o do formul�rio de recarregar a p�gina
    window.location.href = "./forgotPassword.html";
});

// Fun��o para definir um cookie
function setCookie(name, value) {
    //var expires = "";
    //if (days) {
    //    var date = new Date();
    //    date.setTime(date.getTime() + 0 * days * 60 * 60 * 1000);
    //    expires = "; expires=" + date.toUTCString();
    //}
    //document.cookie = name + "=" + (value || "") + expires + "; path=/";

    var date = new Date();
    date.setMinutes(date.getMinutes() + 10);
    // Armazena o valor do cookie e a data de validade no localStorage
    localStorage.setItem(name, JSON.stringify({
        valor: value,
        expiracao: date.getTime()
    }));

}
function makePopUp() {
    var error =  document.getElementById("loginErro");
    error.style.display = 'block';
    setTimeout(function() {
        error.style.display = 'none';
    }, 1500);
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
    var containerElement = document.querySelector('.bodylogin');
    containerElement.appendChild(toastElement);
  
    // Define o tempo para a mensagem toast desaparecer (opcional)
    setTimeout(function () {
      toastElement.remove();
    }, 3000); // A mensagem toast será removida após 3 segundos (3000 milissegundos)
    
  }