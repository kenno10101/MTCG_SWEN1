# create/register users
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"msmith\", \"password\":\"sample2\", \"name\":\"Mary Smith\", \"email\":\"mary.smith@example.com\"}"
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"jdoe\", \"password\":\"example1\", \"name\":\"John Doe\"}"

# register fail
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"jdoe\", \"password\":\"none\", \"name\":\"Oswin Oswald\"}"

# login
curl -i -X POST http://localhost:12000/sessions --header "Content-Type: application/json" -d "{\"username\":\"jdoe\", \"password\":\"example1\"}"

# get all users
curl -i -X GET http://localhost:12000/users --header "Content-Type: application/json"