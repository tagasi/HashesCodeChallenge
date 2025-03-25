# Hashes Code Challenge
# Solution Overview and Deployment

This solution consists of two C# .NET 8.0 projects:

- **Background Worker** - Located in the `HashesReceiver` folder
- **Web API** - Located in the `HashesWebAPI` folder

Both projects include respective Dockerfile configurations, enabling easy build and deployment in Docker containers. Additionally, **RabbitMQ** and **MariaDB** services are configured in the `docker-compose.yml` file.

To deploy and start all services, use the following command:

```sh
docker compose up -d --build
```

---

## Web API Application

The Web API application provides endpoints for creating and retrieving random SHA-1 hashes stored in a MariaDB database table. The application is accessible locally via port **3000**, which is exposed through Docker deployment.

### Available Endpoints

- **POST** `http://localhost:3000/hashes`
  - Generates **40,000** random SHA-1 hashes and sends them to the RabbitMQ queue named `hashes`.

- **GET** `http://localhost:3000/hashes`
  - Retrieves the count of hashes grouped by date.
  - Uses **MemoryCache** with a **1-hour expiration** to optimize performance.

- **GET** `http://localhost:3000/hashes/invalidate`
  - Invalidates the **MemoryCache** instance.

---

## RabbitMQ and MariaDB Services

Both **RabbitMQ** and **MariaDB** are automatically deployed, configured, and started using Docker Compose.

### RabbitMQ

- Standard ports **5672** (AMQP) and **15672** (Management UI) are exposed to the host.
- The **RabbitMQ Admin Panel** is accessible at:
  - [http://localhost:15672/](http://localhost:15672/)

### MariaDB

- Standard port **3306** is exposed to the host.
- Default credentials for database connection:
  - **Username:** `dev`
  - **Password:** `hash#11`
- The following database objects are automatically created on initialization:
  - **Database:** `codetask`
  - **Table:** `hashes`
  - **Stored Procedure:** `getHashes`

---

## Deployment Notes

Ensure **Docker** and **Docker Compose** are installed before executing the deployment command. The services will run in detached mode (`-d`), and the `--build` flag ensures that fresh images are built before deployment.

To stop all services, use:

```sh
docker compose down
```

For troubleshooting, check logs using:

```sh
docker compose logs -f
```

