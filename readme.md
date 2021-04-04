##  U are U C# WebSockets authentication

For more info, read https://devportal.digitalpersona.com/U.are.U%20SDK%20Developer%20Guide.pdf

## self signed keys in windows
 openssl req -x509 -nodes -days 1095 -newkey rsa:2048 -keyout app.key -out app.crt
 openssl pkcs12 -export -out certificate.pfx -inkey .\app.key -in .\app.crtU