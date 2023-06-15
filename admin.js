document.getElementById('dataForm').addEventListener('submit', function(e) {
    e.preventDefault();
  
    const name = document.getElementById('name').value;
    const sip = document.getElementById('sip').value;
    const num = document.getElementById('num').value;
    const department = document.getElementById('department').value;
    const location = document.getElementById('location').value;
    const email = document.getElementById('email').value;
    const img = document.getElementById('img').value;
  
    const data = {
      "name": name,
      "sip": sip,
      "num": num,
      "department": department,
      "location": location,
      "email": email,
      "img": img
    };
  
    // Enviar os dados para o servidor
  fetch('/salvar-dados', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
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
  
    console.log(data); // Exibindo os dados no console para teste
  });