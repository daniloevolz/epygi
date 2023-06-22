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
                    // Login inv�lido, exibe uma mensagem de erro
                    // Colocar POP UP AQ
                    alert("Credenciais inv�lidas. Por favor, tente novamente.");
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
