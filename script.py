import requests
from faker import Faker

base_url = 'http://localhost:5000'  # Replace with the base URL of your ASP.NET Core application

def login():
    # Perform login to get an authentication token
    login_url = base_url + '/Account/Login'
    data = {
        'Username': 'admin@test.com',
        'Password': 'Admin1!'
    }
    response = requests.post(login_url, data=data)
    response.raise_for_status()
    return response.cookies.get('.AspNetCore.Identity.Application')

def add_country(token, country_name):
    # Add a new country using the API
    add_country_url = base_url + '/Tari/New'
    headers = {
        'Authorization': 'Bearer ' + token
    }
    data = {
        'Nume': country_name
    }
    response = requests.post(add_country_url, headers=headers, data=data)
    response.raise_for_status()

def main():
    token = login()
    faker = Faker()
    countries = [faker.country() for _ in range(10)]  # Generate 10 random country names
    for country in countries:
        add_country(token, country)
        print(f'Added country: {country}')

if __name__ == '__main__':
    main()
