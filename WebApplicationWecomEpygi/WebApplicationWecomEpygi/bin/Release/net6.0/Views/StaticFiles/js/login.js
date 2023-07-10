var cookie;
 
 function load() {
    // exemplo de uso: obtém o valor do cookie "successCookie"
     var successValue = getCookie("successLoginCookie");
    if (successValue != null) {
        window.location.href = "./admin-interf.html";
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
                setCookie("successLoginCookie", data.success, 1);
                // Login bem-sucedido, redireciona para a p�gina autenticada
                window.location.href = "./admin-interf.html";
            } else {
                    // Login inválido, exibe uma mensagem de erro
                    makePopUp();  
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
function setCookie(name, value, days) {
    var expires = "";
    if (days) {
        var date = new Date();
        date.setTime(date.getTime() + days * 24 * 60 * 60 * 1000);
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}
function makePopUp() {
    var error =  document.getElementById("loginErro");
    error.style.display = 'block';
    setTimeout(function() {
        error.style.display = 'none';
    }, 1500);
}