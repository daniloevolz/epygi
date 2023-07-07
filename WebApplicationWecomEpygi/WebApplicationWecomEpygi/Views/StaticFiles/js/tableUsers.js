// tabela de usuários adm
var users = []
 fetch('./users.json')
  .then(response => response.json())
  .then( data => {
     users = data
     users.forEach(function(user){
         var html = `
		<tbody>
			<tr>
            <td><img src="${user.img}" alt="" style="width:35px ;height:35px" ></td>
            <td>${user.name}</td>
            <td style="text-transform: capitalize; text-align: center;">${user.department}</td>
            <td style="text-align: center;"><strong>${user.sip}</strong></td>
            <td>${user.email}</td>
            <td style="text-transform: capitalize; text-align: center;">${user.num}</td>
            <td style="text-transform: capitalize; text-align: center;">${user.location}</td>
			</tr>
		</tbody>
       `;
      
    //   document.getElementById("div-users").innerHTML = '';
      document.getElementById("data-table").innerHTML += html
       })
       
    })
   .catch(error => {
     console.error('Erro ao carregar o arquivo JSON', error);
});