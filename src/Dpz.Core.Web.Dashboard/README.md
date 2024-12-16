### nginx
```conf
server {
    listen                      80;
    listen                      443 ssl http2;
    server_name                 manage.dpangzi.com;
    ssl_certificate             /home/ubuntu/cert/manage/manage.dpangzi.com_bundle.pem;
    ssl_certificate_key         /home/ubuntu/cert/manage/manage.dpangzi.com.key;
    ssl_protocols               TLSv1.1 TLSv1.2 TLSv1.3;
    ssl_ciphers                 EECDH+CHACHA20:EECDH+CHACHA20-draft:EECDH+AES128:RSA+AES128:EECDH+AES256:RSA+AES256:EECDH+3DES:RSA+3DES:!MD5;
    ssl_prefer_server_ciphers   on;
    ssl_session_cache           shared:SSL:10m;
    ssl_session_timeout         10m;
    add_header                  Strict-Transport-Security "max-age=31536000";
    error_page 497              https://$host$request_uri;
    root                        /home/ubuntu/program/manage/wwwroot;
    location / {
        root                    /home/ubuntu/program/manage/wwwroot;
        try_files               $uri $uri/ /index.html = 404;
        limit_req               zone=one burst=60 nodelay;
    }
  }
```