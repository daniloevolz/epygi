
var cookie;
function load() {
    // Exemplo de uso: obtém o valor do cookie "successCookie"
    var successValue = getCookie("successLoginCookie");
    if (successValue == null) {
        window.location.href = "./login.html";
    } else {
        cookie = successValue;
    }
}


document.getElementById('a-upload-user').addEventListener('click', function (e) {
    e.preventDefault();

    const name = document.getElementById('name').value;
    const sip = document.getElementById('sip').value;
    const num = document.getElementById('num').value;
    const department = document.getElementById('department').value;
    const location = document.getElementById('location').value;
    const email = document.getElementById('email').value;
    const imgFile = document.getElementById('ipt-img-user').files[0]; // Obter o arquivo de imagem

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
            "data": null // Será preenchido posteriormente
        }
    };

    // Ler o conteúdo da imagem usando FileReader
    const reader = new FileReader();
    reader.onload = function (event) {
        data.image.data = event.target.result; // Definir o conteúdo da imagem no objeto de dados
        // Enviar os dados para o servidor
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
    reader.readAsDataURL(imgFile); // Ler o conteúdo da imagem como base64
});

// Função para obter o valor de um cookie pelo nome
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


