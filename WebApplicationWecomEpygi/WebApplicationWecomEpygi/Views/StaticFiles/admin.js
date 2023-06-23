
 var cookie;
//  function load() {
//     // exemplo de uso: obtém o valor do cookie "successCookie"
//      var successValue = getCookie("successLoginCookie");
//     if (successValue == null) {
//         window.location.href = "./login.html";
//    } else {
//        cookie = successValue;
//    }
//  }
 
 var users = []
 fetch('./users.json')
  .then(response => response.json())
  .then( data => {
     users = data
 
     users.forEach(function(user){
         var html = `
		<tbody>
			<tr>
            <td><img src="${user.img}" alt="" style="width:35px ;height:25px" ></td>
            <td>${user.name}</td>
            <td>${user.department}</td>
            <td><strong>${user.sip}</strong></td>
            <td>${user.email}</td>
            <td>${user.num}</td>
            <td>${user.location}</td>
			</tr>
		</tbody>
	</table>
</div>
       `;
      
    //   document.getElementById("div-users").innerHTML = '';
      document.getElementById("table-users").innerHTML += html
       })
       
    })
   .catch(error => {
     console.error('Erro ao carregar o arquivo JSON', error);
   });


document.getElementById('a-upload-user').addEventListener('click', function (e) {
    e.preventDefault();

    const name = document.getElementById('name').value;
    const sip = document.getElementById('sip').value;
    const num = document.getElementById('num').value;
    const department = document.getElementById('department').value.toLowerCase();
    const location = document.getElementById('location').value;
    const email = document.getElementById('email').value;
    const imgFile = document.getElementById('ipt-img-user').files[0]; // obter o arquivo de imagem

    
    if (name === '' || sip === '' || num === '' || department === '' || location === '' || email === '' || !imgFile) {
        makePopUp();
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

    showUsersDiv();
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
function showUsersDiv(){
    document.getElementById('name').value = ''
    document.getElementById('sip').value = ''
    document.getElementById('num').value = ''
    document.getElementById('department').value = ''
    document.getElementById('location').value = ''
    document.getElementById('email').value = ''
    document.getElementById('ipt-img-user').value = '';
    
    document.getElementById("div-users").style.display = 'block';
    document.getElementById("addUsers").style.display = 'none';
}
document.getElementById("show-users").addEventListener("click",showUsersDiv)

document.getElementById("useradd").addEventListener("click",function(){
    document.getElementById("addUsers").style.display = 'block';
    document.getElementById("div-users").style.display = 'none';
    
})
function makePopUp(){
    var error = document.getElementById("loginErro");
    error.style.display = 'block';
    setTimeout(function(){
        error.style.display = 'none'
    },1200)

}




