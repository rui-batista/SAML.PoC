# Introduction

This project is a collection of *proof of concept* implementations of SSO with SAML protocols for various versions of .NET frameworks.

1. The first implementation uses ITfoxtec to implement SAML and follows a post from [morioh.com](https://morioh.com/p/78ee005c07cc) and the ITfoxtec samples.
For this inplementation, project `SAML.PoC.IdP` plays the role of a (very) simple IdP (Identity Provider) and project `SAML.PoC.SP1` the role of a basic .NET MVC service provider.

2. **WIP:** The second implementation uses Keycloak as Identity Provider, and the project `SAML.PoC.SP2` as a .NET MVC Service Provider.

3. **TODO:** The third implmentation will use WIF (Windows Identity Foundation) to implmement SAML.

4. **TODO:** The fourth implmentation will use OWIN (Open Web Interface for .NET) to implement SAML.

This is a Work in Progress that will be updated as I work trough the planned implementations. I will try to document all configurations and problems found.


# Getting started

To get the first PoC started, both the `SAML.PoC.IdP` and `SAML.PoC.SP1` must be running at the same time.

1. In Visual Studio, right-click the solution, select the *Set Startup Projects...* option, then check the *Multiple startup projects* bullet and select start in both `SAML.PoC.IdP` and `SAML.PoC.SP1` projects. Some pop-ups will prompt you about the SSL and certificate installation and the default browser should open with the index page of both started projects.  
Pay attention to runtime ports for each project, they are hard-coded in appsettings.json and can differ from the original.
2. Follow the SP1 index page instructions to authenticate.

The second PoC, `SAML.PoC.SP2`. requires a running minimal Keycloak identity provider to work. I used docker to setup and run a keycloak instance with a mysql database. The implementation files can be found in the *Keycloak* folder in the solution.

To setup keycloak for the `SAML.PoC.SP2`, a realm named *Teste* must be created. Then you will need to create a client named *mysamlapp* and configured with SAML as *Client Protocol* and Client SAML Endpoint as **https://localhost:5001/Auth/AssertionConsumerService** (the `SAML.PoC.SP2` AssertionConsumerService - host and port my be different in your implementation so this might need to be adjusted).

> To avoid configuring signing certificates, go to the client *Settings* and disable the *Client Signature Required*. I am aware that signing is the cornerstone of SAML security, but the scope of this proof of concept is to research available SAML authentication tools for .NET applications.

Two roles need to be added to this client: *role1* and *role2*. Then you will need a User with at least one of those roles set: `SAML.PoC.SP2` has one View that requires *role1* and another that requires *role2*.

> I have provided a realm export file in the keycloak folder for convenience. Note that keycloak will not export passwords nor secrets, Those will saved as '**********'.

Unsolved issues:
1. SingleLogout is not working with Keycloak.
2. Needs a proper Error page or redirect when a user without the required role tries to load a restricted view.


# Reference links

## What is SAML?
> https://duo.com/blog/the-beer-drinkers-guide-to-saml

## ITfoxtec Identity SAML 2

> https://www.itfoxtec.com/IdentitySaml2
>
> https://morioh.com/p/78ee005c07cc
>
> https://developer.okta.com/blog/2020/10/23/how-to-authenticate-with-saml-in-aspnet-core-and-csharp

## WIF (Windows Identity Foundation)

> https://stackoverflow.com/questions/15530184/working-with-saml-2-0-in-c-sharp-net-4-5
>
> https://docs.microsoft.com/en-us/previous-versions/dotnet/framework/windows-identity-foundation/?redirectedfrom=MSDN

## OWIN (Open Web Interface for .NET)

> https://docs.microsoft.com/en-us/azure/active-directory/develop/tutorial-v2-asp-webapp
>
> https://www.devmedia.com.br/asp-net-identity-como-trabalhar-com-owin/33003
>
> https://magicinlogic.blogspot.com/2021/02/how-to-implement-keycloak.html
>
> https://github.com/BasLijten/EmbeddedStsSample
>
> implementation example:
> https://blog.baslijten.com/how-to-setup-a-simple-sts-for-web-application-development/


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

### Keycloak REST API
> https://www.baeldung.com/postman-keycloak-endpoints
>
> https://developers.redhat.com/blog/2020/11/24/authentication-and-authorization-using-the-keycloak-rest-api#keycloak_connection_using_a_java_application


### Keycloak using SQL Server

To use sql server for Keycloak, XA (extended architecture) must be enabled, then a role must be created and the keycloak db user added to that role.

> ref:
> https://www.frodehus.dev/setting-up-keycloak-with-ms-sql-server-2019/

Experimenting with the Dockerfile command, trying to get it done but still some issues...
```YAML
 command: ['/bin/bash', '-c', 'until /opt/mssql-tools/bin/sqlcmd -S database -U sa
 -P "DefaultPassword123" -Q "EXEC sp_sqljdbc_xa_install; CREATE DATABASE keycloak;
 CREATE LOGIN keycloak WITH PASSWORD = ''lol!Iusepasswords''; USE master; EXEC
 sp_grantdbaccess ''keycloak'', ''keycloak''; EXEC sp_addrolemember [SqlJDBCXAUser],
 ''keycloak''; USE keycloak; CREATE USER keycloak FOR LOGIN keycloak; EXEC
 sp_addrolemember N''db_owner'', N''keycloak''" do sleep 5; done']
```

As keycloack runs out-of-the-box with mysql, I used that one for the docker composition and will explore the use of all other well known database engines at a later time.

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
Select *Groups*, double click the group, then *Role Mappings*, select the client who has the roles defined and add the roles you want assigned to this group. Then add or remove users from the group.

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


https://stackoverflow.com/questions/69727838/itfoxtec-saml-2-0-single-logout
https://stackoverflow.com/questions/44712576/single-sign-out-principle-in-keycloak

# Contribute

I created this project to document and report my research about SSO with SAML authentication, in preparation to implement it in a client's old .NET 4.5 application. Collaboration is not expected, but I am always ready to learn more. So if anyone wants to add any knowledge, point in better directions or even correct some wrongs, please feel free to participate.

last updated: 2022-07-18 10:35:37