# Introduction

This project is a collection of *proof of concept* implementations of SSO with SAML protocols for various versions of .NET frameworks.

1. The first implementation uses ITfoxtec to implement SAML and follows a post from [morioh.com](https://morioh.com/p/78ee005c07cc) and the ITfoxtec samples.
For this inplementation, project SAML.PoC.IdP plays the role of a (very) simple IdP (Identity Provider) and project SAML.PoC.SP1 the role of a basic .NET MVC service provider.

2. **WIP:** The second implementation uses Keycloak as Identity Provider, and the project SAML.PoC.SP2 as a .NET MVC Service Provider.

3. **TODO:** The third implmentation will use WIF (Windows Identity Foundation) to implmement SAML.

4. **TODO:** The fourth implmentation will use OWIN (Open Web Interface for .NET) to implement SAML.

This is a Work in Progress that will be updated as I work trough the planned implementations. I will try to document all configurations and problems found.


# Getting started

To get the first PoC started, both the SAML.PoC.IdP and SAML.PoC.SP1 must be running at the same time.

1. In Visual Studio, right-click the solution, select the *Set Startup Projects...* option, then check the *Multiple startup projects* bullet and select start in both SAML.PoC.IdP and SAML.PoC.SP1 projects. Some pop-ups will prompt you about the SSL and certificate installation and the default browser should open with the index page of both started projects.  
Pay attention to runtime ports for each project, they are hard-coded in appsettings.json and can differ from the original.
2. Follow the SP1 index page instructions to authenticate.

The second PoC, SAML.PoC.SP2. requires a running minimal Keycloak identity provider to work. I used docker to setup and run a keycloak with a mysql database persisted in a filesystem volume. The implementation files can be found in the *Keycloak* folder in teh solution.

To setup keycloak for the SAML.PoC.SP2, a realm named *Teste* must be created. Then you will need to create a client named *mysamlapp* and configured with SAML as *Client Protocol* and Client SAML Endpoint as **https://localhost:5001/Auth/AssertionConsumerService** (the SAML.PoC.SP2 AssertionConsumerService, host and port my be different in you implementation so this might need to be adjusted).
To avoid configuring signature certificates, go to the client Settings and disable the *Client Signature Required*. I am aware that signing is the cornerstone of SAML security, but the scope of this proof if concept is to implement SAML authentication with .NET applications.
Two roles, *role1* and *role2* need to be added to this client. The you will need a client with at least one of those roles set.
I have provided a realm export file in the keycloak folder for convenience. Note that the keycloak will not export password nor secrets, they will saved as '**********'.


# Reference notes

## SAML
> https://duo.com/blog/the-beer-drinkers-guide-to-saml

## ITfoxtec.Identity.Saml2

> https://www.itfoxtec.com/IdentitySaml2
>
> https://morioh.com/p/78ee005c07cc
>
> https://developer.okta.com/blog/2020/10/23/how-to-authenticate-with-saml-in-aspnet-core-and-csharp


## WIF (Windows Identity Foundation)
> https://stackoverflow.com/questions/15530184/working-with-saml-2-0-in-c-sharp-net-4-5
>
> https://docs.microsoft.com/en-us/previous-versions/dotnet/framework/windows-identity-foundation/?redirectedfrom=MSDN
>
> WIF implementation example:
> https://blog.baslijten.com/how-to-setup-a-simple-sts-for-web-application-development/


## OWIN (Open Web Interface for .NET)

> https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-asp-webapp
>
> https://www.devmedia.com.br/asp-net-identity-como-trabalhar-com-owin/33003
>
> https://magicinlogic.blogspot.com/2021/02/how-to-implement-keycloak.html
>
> https://github.com/BasLijten/EmbeddedStsSample


## Custom HTTP Modules

> https://docs.microsoft.com/en-us/previous-versions/aspnet/ms227673(v=vs.100)


## Keycloak

> https://www.keycloak.org/getting-started/getting-started-docker
>
> https://gitlab.pflaeging.net/pflaeging-net-public/keycloak-compose/-/tree/main
>
> https://www.frodehus.dev/setting-up-keycloak-with-ms-sql-server-2019/
>
> https://github.com/keycloak/keycloak-containers/blob/main/docker-compose-examples/keycloak-mssql.yml
>
> https://iamvickyav.medium.com/mysql-init-script-on-docker-compose-e53677102e48
>
> https://www.softwaredeveloper.blog/initialize-mssql-in-docker-container
>
> https://shami.blog/2021/07/howto-build-a-keycloak/ubuntu/mariadb-cluster-without-multicast-udp/
>
> https://manhng.com/blog/keycloak/

### OAuth 2.0 & OpenID REST APIs:
> https://www.baeldung.com/postman-keycloak-endpoints

### testing
> https://developers.redhat.com/blog/2020/11/24/authentication-and-authorization-using-the-keycloak-rest-api#keycloak_connection_using_a_java_application


### Keycloak using MySQL

```Dockerfile
FROM quay.io/keycloak/keycloak:18.0.0 as builder

ENV KC_HEALTH_ENABLED=true
ENV KC_METRICS_ENABLED=true
ENV KC_FEATURES=token-exchange
ENV KC_DB=mysql
# Install custom providers
RUN curl -sL https://github.com/aerogear/keycloak-metrics-spi/releases/download/2.5.3/keycloak-metrics-spi-2.5.3.jar -o /opt/keycloak/providers/keycloak-metrics-spi-2.5.3.jar
RUN /opt/keycloak/bin/kc.sh build

FROM quay.io/keycloak/keycloak:18.0.0
COPY --from=builder /opt/keycloak/ /opt/keycloak/

# for demonstration purposes only, please make sure to use proper certificates in production instead
# RUN keytool -genkeypair -storepass password -storetype PKCS12 -keyalg RSA -keysize 2048 -dname "CN=server" -alias server -ext "SAN:c=DNS:localhost,IP:127.0.0.1" -keystore conf/server.keystore

#ENTRYPOINT ["/opt/keycloak/bin/kc.sh", "start"]
```

```YAML
version: "3.9"

networks:
    default-network:
        name: keycloak
        driver: bridge

services:
    keycloak:
        container_name: Keycloak
        build: .
        image: keycloak_mysql
        command: ['start', '--hostname-strict', 'false', '--log-level', 'INFO']
        environment:
            KC_DB_URL: jdbc:mysql://database/keycloak
            KC_DB_USERNAME: keycloak
            KC_DB_PASSWORD: KeyCloak

            KC_HOSTNAME: localhost
            PROXY_ADDRESS_FORWARDING: true

            KC_HTTPS_KEY_STORE_FILE: /opt/keycloak/conf/keycloak.keystore
            KC_HTTPS_KEY_STORE_PASSWORD: password

            KEYCLOAK_ADMIN: admin
            KEYCLOAK_ADMIN_PASSWORD: admin
        volumes:
            - ./keycloak.keystore:/opt/keycloak/conf/keycloak.keystore
        depends_on:
            - database
        ports:
            - 8443:8443
            - 8080:8080
        networks:
            - default-network
        healthcheck:
            test: ["CMD", "curl" ,"--fail", "https://localhost:8443/realms/master"]
            interval: 30s
            timeout: 3s
            retries: 3
        restart: always

    database:
        container_name: MySQL
        image: mysql:8.0
        environment:
            MYSQL_ROOT_PASSWORD: root
            MYSQL_DATABASE: keycloak
            MYSQL_USER: keycloak
            MYSQL_PASSWORD: KeyCloak
        volumes:
            - ./mysql_data:/var/lib/mysql
        ports:
            - 13306:3306
        networks:
            - default-network
        healthcheck:
            test: ["CMD", "mysqladmin", "ping", "--silent"]
            interval: 10s
            timeout: 20s
            retries: 10
        restart: always
```


### Keycloak using SQL Server

```YAML
command: ['start', '--hostname-strict', 'false', '--log-level', 'INFO',
'--db-url-host', '192.168.1.1:32775', '--db-username', 'sa', '--db-password',
'DefaultPassword123']
```

To use sql server for Keycloak, XA (extended architecture) must be enabled, a role created and keycloak db user must be added to that role. This next command is a draft to be used in the dockerfile for that, but sice mysql worked immediatelly without extra work, sql server docker configuration will be studied later. I actually managed to have it working, but had to setup XA by hand first.

> ref:
> https://www.frodehus.dev/setting-up-keycloak-with-ms-sql-server-2019/

```YAML
 command: ['/bin/bash', '-c', 'until /opt/mssql-tools/bin/sqlcmd -S database -U sa
 -P "DefaultPassword123" -Q "EXEC sp_sqljdbc_xa_install; CREATE DATABASE keycloak;
 CREATE LOGIN keycloak WITH PASSWORD = ''lol!Iusepasswords''; USE master; EXEC
 sp_grantdbaccess ''keycloak'', ''keycloak''; EXEC sp_addrolemember [SqlJDBCXAUser],
 ''keycloak''; USE keycloak; CREATE USER keycloak FOR LOGIN keycloak; EXEC
 sp_addrolemember N''db_owner'', N''keycloak''" do sleep 5; done']
```

### mapping claims to roles
> https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-6.0

Keycloak sets the role claim type as "role" and .NET won't pick that up as a role, needs it to be mapped to ClaimTypes.Role, that will become "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", so in the ClaimsTransform.cs static class, I replaced the line

```CS
claims.AddRange(incomingPrincipal.Claims);
```

with

```CS
foreach (var claim in incomingPrincipal.Claims)
{
    if (claim.Type == "Role")
    {
        claims.Add(new Claim(ClaimTypes.Role, claim.Value));
    }
    else
    {
        claims.Add(claim);
    }
}
```

That seems enough for the .NET recon the roles annotations in MVC controllers, like 

```CS
[Authorize(Roles = "role1,role2")] 
```


### Using Keycloak Groups for role granting
> Select *Groups*, double click the group, then *Role Mappings*, select the client who has the roles defined, and add the roles you want assigned to this group. Then you add or remove users from the group.

### Certificates...

> https://stackoverflow.com/questions/44066709/your-connection-is-not-private-neterr-cert-common-name-invalid
>
> https://www.feistyduck.com/library/openssl-cookbook/online/
>
> https://security.stackexchange.com/questions/29425/difference-between-pfx-and-cert-certificates

### InvalidSignatureException: Signature is invalid

> https://stackoverflow.com/questions/58603633/invalidsignatureexception-signature-is-invalid
> https://tkit.dev/2020/05/25/a-potential-fix-for-itfoxtech-identity-saml2-signature-is-invalid-error/
> https://devforum.okta.com/t/signature-is-invalid-error-on-saml-integration/11931
> https://keycloak.discourse.group/t/invalid-signature-with-hs256-token/3228/9
> https://github.com/nextcloud/server/issues/17403

# Contribute

I created this project to document and report my research about SSO with SAML authentication, in preparation to implement it in a client's old .NET 4.5 application. Collaboration is not expected, but I am always ready to learn more. So if anyone wants to add any knowledge, point in better directions, or even correct some wrongs, feel free to participate.

last updated: 2022-07-04 12:09:47