# 1
## 1.a
app:
    ...
    ports:
      - "7081:8080"

app-ro1:
    ...
    ports:
      - "7082:8080"     

app-ro2:
    ...
    ports:
      - "7083:8080"  
## 1.b
upstream backend_get {
    server app:8080 weight=2;
    server app-ro1:8080 weight=1;
    server app-ro2:8080 weight=1;
}
## 1.c
### coffeeshops.conf
map $request_method $api_upstream {
    default backend_write;
    GET backend_get;
    HEAD backend_get;
    OPTIONS backend_get;
}
upstream backend_get {
    server app:8080 weight=2;
    server app-ro1:8080 weight=1;
    server app-ro2:8080 weight=1;
}

upstream backend_write {
    server app:8080;
}

### docker-compose.yml
app:
    ...
      ReadOnlyMode: "false"
app-ro1:
    ...
      ReadOnlyMode: "true"
app-ro2:
    ...
      ReadOnlyMode: "true"
### Program.cs
var readOnlyMode = builder.Configuration.GetValue<bool>("ReadOnlyMode");
var instanceName = builder.Configuration.GetValue<string>("InstanceName") ?? "main";
...

## 1.d
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Instance-Name"] = instanceName;
    context.Response.Headers["Server"] = "CoffeeShops";

    var isWriteMethod = !HttpMethods.IsGet(context.Request.Method)
        && !HttpMethods.IsHead(context.Request.Method)
        && !HttpMethods.IsOptions(context.Request.Method);

    if (readOnlyMode && isWriteMethod)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            error = "Instance runs in read-only mode",
            instance = instanceName
        });
        return;
    }

    await next();
});



## проверка read only
Проверка записи только на master:
`curl -I -X POST http://localhost:7080/api/v1/companies` --> 401 (авторизация требуется, X-Instance-Name: main).

`curl -I -X POST http://localhost:7080/mirror/api/v1/companies` или к ro1/ro2 (7082/7083) --> 403 (read-only срабатывает).

`curl -i -X DELETE http://localhost:7080/api/v1/companies/1` несколько раз -- ответы всегда 401/403 только с `X-Instance-Name: main`

`curl -i -X DELETE http://localhost:7080/mirror/api/v1/companies/1` -- 403 + тело `{error, instance}`

# 2
## утилита Apache Benchmark
lus@lusix:~/src_PORTS$ ab -n 200 -c 20 hab -n 200 -c 20 http://localhost:7080/api/v1/companies
This is ApacheBench, Version 2.3 <$Revision: 1879490 $>
Copyright 1996 Adam Twiss, Zeus Technology Ltd, http://www.zeustech.net/
Licensed to The Apache Software Foundation, http://www.apache.org/

Benchmarking localhost (be patient)
Completed 100 requests
Completed 200 requests
Finished 200 requests


Server Software:        CoffeeShops
Server Hostname:        localhost
Server Port:            7080

Document Path:          /api/v1/companies
Document Length:        2549 bytes

Concurrency Level:      20
Time taken for tests:   0.406 seconds
Complete requests:      200
Failed requests:        0
Total transferred:      547698 bytes
HTML transferred:       509800 bytes
Requests per second:    492.47 [#/sec] (mean)
Time per request:       40.611 [ms] (mean)
Time per request:       2.031 [ms] (mean, across all concurrent requests)
Transfer rate:          1317.03 [Kbytes/sec] received

Connection Times (ms)
              min  mean[+/-sd] median   max
Connect:        0    0   0.2      0       1
Processing:     7   39  28.2     31     118
Waiting:        5   32  22.9     26     108
Total:          7   39  28.2     31     118

Percentage of the requests served within a certain time (ms)
  50%     31
  66%     53
  75%     62
  80%     66
  90%     80
  95%     88
  98%    100
  99%    113
 100%    118 (longest request)


## проверка балансировки
lus@lusix:~/src_PORTS$ ./check_balance.sh -n 200 -u http://localhost:7080/api/v1/companies
Results for 200 requests to http://localhost:7080/api/v1/companies
ro1        50
ro2        50
main       100



lus@lusix:~/src_PORTS$ ./check_balance_new.sh -n 20 -m GET -u "/api/v1/coffeeshops"
Testing 20 GET requests to: http://localhost:7080/api/v1/coffeeshops
==============================================

Results:
========
ro1        5
ro2        5
main       10

lus@lusix:~/src_PORTS$ ./check_balance_new.sh -n 10 -m POST -u "/api/v1/companies"
Testing 10 POST requests to: http://localhost:7080/api/v1/companies
==============================================

Results:
========
main       10

# 3
## маршрутизация на mirror
app-mirror:
   ....
      ASPNETCORE_PATHBASE: "/mirror"
      PathBase: "/mirror"
      InstanceName: mirror
      ReadOnlyMode: "true"
      ConnectionStrings__DefaultConnection: "Host=db-replica;Port=5432;Database=web_course;Username=guest;Password=guest2025"
      ConnectionStrings__UserConnection: "Host=db-replica;Port=5432;Database=web_course;Username=guest;Password=guest2025"
      ...
    labels:
      AppInstance: mirror
      Role: read
    ...


Файл: `nginx/conf.d/coffeeshops.conf` — `location /mirror/` проксирует на `backend_mirror`, ставит `X-Forwarded-Prefix: /mirror`

### проверка
lus@lusix:~/src_PORTS$ curl -I http://localhost:7080/mirror/api/v1/companies
HTTP/1.1 200 OK
Date: Sat, 22 Nov 2025 23:05:46 GMT
Content-Type: application/json; charset=utf-8
Connection: keep-alive
X-Instance-Name: mirror
Server: CoffeeShops
X-Cache-Status: BYPASS

## Репликация БД
db:
    image: bitnami/postgresql:latest
    environment:
      - POSTGRESQL_REPLICATION_MODE=master
      - POSTGRESQL_USERNAME=postgres
      - POSTGRESQL_PASSWORD=lucy2004
      - POSTGRESQL_DATABASE=web_course
      - POSTGRESQL_REPLICATION_USER=replicator
      - POSTGRESQL_REPLICATION_PASSWORD=replicator2025
      ...

db-replica:
    image: bitnami/postgresql:latest
    environment:
      - POSTGRESQL_REPLICATION_MODE=slave
      - POSTGRESQL_PRIMARY_HOST=db
      - POSTGRESQL_PRIMARY_PORT_NUMBER=5432
      - POSTGRESQL_PRIMARY_USER=postgres
      - POSTGRESQL_PRIMARY_PASSWORD=lucy2004
      - POSTGRESQL_MASTER_HOST=db
      - POSTGRESQL_MASTER_PORT_NUMBER=5432
      - POSTGRESQL_MASTER_USER=postgres
      - POSTGRESQL_MASTER_PASSWORD=lucy2004
      - POSTGRESQL_REPLICATION_USER=replicator
      - POSTGRESQL_REPLICATION_PASSWORD=replicator2025
      - POSTGRESQL_USERNAME=postgres
      - POSTGRESQL_PASSWORD=lucy2004
    depends_on:
      db:
        condition: service_healthy
    ...

Инициализация: `init-db/005-create-guest-role.sql` создает роль `guest` с GRANT SELECT и default privileges; read-инстансы подключаются к `db-replica` под `guest`
Переменные подключения: у `app-ro1/ro2/mirror` строки вида `Host=db-replica;Username=guest;Password=guest2025`

### проверка
f (false) = это MASTER база данных

lus@lusix:~/src_PORTS$ docker compose exec db psql -U postgres -d web_course -c "select pg_is_in_recovery()"
WARN[0000] /home/lus/src_PORTS/docker-compose.yml: the attribute `version` is obsolete, it will be ignored, please remove it to avoid potential confusion 
Password for user postgres: 
 pg_is_in_recovery 
-------------------
 f
(1 row)

===================================
on (true) = это SLAVE база данных

lus@lusix:~/src_PORTS$ docker compose exec db-replica psql -U postgres -d web_course -c "show transaction_read_only"
WARN[0000] /home/lus/src_PORTS/docker-compose.yml: the attribute `version` is obsolete, it will be ignored, please remove it to avoid potential confusion 
Password for user postgres: 
 transaction_read_only 
-----------------------
 on
(1 row)

=========================================
Попытка записи на реплику

lus@lusix:~/src_PORTS$ docker compose exec db-replica psql -U guest -d web_course -c "insert into companies(name) values('x')"
WARN[0000] /home/lus/src_PORTS/docker-compose.yml: the attribute `version` is obsolete, it will be ignored, please remove it to avoid potential confusion 
Password for user guest: 
ERROR:  cannot execute INSERT in a read-only transaction


## проверка /
версия с записью и чтением:
http://localhost:7080/ в адресной строке в браузере вбить
http://localhost:7080/api/v1/

версия с только чтением:
http://localhost:7080/mirror/ 
http://localhost:7080/mirror/api/v1/

# 4
## nginx.conf:
server_tokens off;
more_clear_headers Server;
more_set_headers "Server: CoffeeShops";




## проверка 
lus@lusix:~/src_PORTS$ curl -I http://localhost:7080/api/v1/companies
HTTP/1.1 200 OK
Date: Sat, 22 Nov 2025 23:38:37 GMT
Content-Type: application/json; charset=utf-8
Connection: keep-alive
X-Instance-Name: main
Server: CoffeeShops  (ТУТ)
X-Cache-Status: BYPASS


# 5 Кэширование и сжатие
## nginx.conf:
gzip on;
gzip_proxied any;
gzip_comp_level 5;
gzip_min_length 256;
gzip_types text/plain text/css application/json application/javascript application/xml+rss application/xml text/javascript;

proxy_cache_path /var/cache/nginx levels=1:2 keys_zone=app_cache:50m max_size=500m inactive=10m use_temp_path=off;
proxy_cache_path /var/cache/static levels=1:2 keys_zone=static_cache:10m max_size=100m inactive=1h use_temp_path=off;

## проверка gzip
lus@lusix:~/src_PORTS$ curl -I -H "Accept-Encoding: gzip" http://localhost:7080/api/v1/companies
HTTP/1.1 200 OK
Date: Sat, 22 Nov 2025 23:40:58 GMT
Content-Type: application/json; charset=utf-8
Connection: keep-alive
X-Instance-Name: main
Server: CoffeeShops
X-Cache-Status: BYPASS
Content-Encoding: gzip

## проверка кэширования
#Очистить кэш 
docker exec src_ports-nginx-1 find /var/cache/static -type f -delete

#ПЕРВЫЙ ЗАПРОС (должен быть MISS):"
curl -s -I http://localhost:7080/css/log_reg_style.css | grep "X-Cache-Status"

#ВТОРОЙ ЗАПРОС (должен быть HIT):"
curl -s -I http://localhost:7080/css/log_reg_style.css | grep "X-Cache-Status"

#ТРЕТИЙ ЗАПРОС (должен быть HIT):"
curl -s -I http://localhost:7080/css/log_reg_style.css | grep "X-Cache-Status"
    
# 6 Настроить по пути /monitoring связку Grafana Loki, где разместить визуализацию логов приложения со всех инстансов
- Файлы: `monitoring/loki-config.yml`, `monitoring/promtail-config.yml`, `monitoring/grafana/provisioning/*`, маршрут `/monitoring/` в `nginx/conf.d/coffeeshops.conf`.
- Promtail собирает:
  - Docker контейнеры (service labels → лог-лейблы `service`, `instance`, `role`).
  - Файлы `/var/log/app/*/coffeeshops-*.txt` (app-логи), `/var/log/app/nginx/*.log`.
- Loki стартует с конфигом из `loki-config.yml`; Grafana имеет datasource Loki и дашборд `logs.json`.



## проверка (пароль admin и логи admin)
http://localhost:7080/monitoring/
или
http://localhost:7090/monitoring/