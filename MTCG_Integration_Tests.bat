echo ========================================
echo create/register users
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"ksanga\", \"password\":\"sample2\", \"name\":\"Mary Smith\", \"email\":\"mary.smith@example.com\"}"
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"jdoe\", \"password\":\"example1\", \"name\":\"John Doe\"}"

echo ========================================
echo register fail
curl -i -X POST http://localhost:12000/users --header "Content-Type: application/json" -d "{\"username\":\"ksanga\", \"password\":\"none\", \"name\":\"Kenn Sanga\"}"

echo ========================================
echo login and get token
curl -i -X POST http://localhost:12000/sessions --header "Content-Type: application/json" -d "{\"username\":\"ksanga\", \"password\":\"sample2\"}"

echo ========================================
echo login fail
curl -i -X POST http://localhost:12000/sessions --header "Content-Type: application/json" -d "{\"username\":\"jdoe\", \"password\":\"fail\"}"

echo ========================================
echo get user ksanga
curl -i -X GET http://localhost:12000/users/ksanga --header "Content-Type: application/json" --header "Authorization: Bearer ksanga-debug"

echo ========================================
echo update user ksanga
curl -i -X PUT http://localhost:12000/users/ksanga --header "Content-Type: application/json" --header "Authorization: Bearer ksanga-debug" -d "{\"password\":\"example1\", \"name\":\"Kenn Sanga\"}"

echo ========================================
echo get user ksanga
curl -i -X GET http://localhost:12000/users/ksanga --header "Content-Type: application/json" --header "Authorization: Bearer ksanga-debug"

echo ========================================
echo update user ksanga username
curl -i -X PUT http://localhost:12000/users/ksanga --header "Content-Type: application/json" --header "Authorization: Bearer ksanga-debug" -d "{\"username\":\"fredz\", \"name\":\"Fred Zinnemann\"}"

echo ========================================
echo get user ksanga fail
curl -i -X GET http://localhost:12000/users/ksanga --header "Content-Type: application/json" --header "Authorization: Bearer ksanga-debug"

echo ========================================
echo get user fredz
curl -i -X GET http://localhost:12000/users/fredz --header "Content-Type: application/json" --header "Authorization: Bearer fredz-debug"