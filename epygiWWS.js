function getUsersStatus(url, data, callback) {
    const url = 'https://example.com/api'; // Substitua pela URL real
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
        const users = jsonData.users;
        
        for (const username in users) {
          const status = users[username];
          console.log(`${username}: ${status}`);

        }
        callback(users);
      })
      .catch(error => {
        console.error('Erro ao fazer a requisição:', error);
      });
  }
  function updateUserStatus(){



  };
  updateUserStatus;
  getUsersStatus();
  