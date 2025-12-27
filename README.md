# Pyatform

This is a backend service of Pyatform - coding challange platform with execution and evalutaion engine. It handles challenge management, secure code execution, user progress tracking, and asset storage.

Users submit Python challenge solutions, which are safely executed inside Docker-in-Docker containers and validated using pytest. The backend exposes a REST API consumed by the [Next.js frontend.](https://github.com/arekminajj/pyatform-frontend)

## Features 
- Automated python code evaluation with pytest
- Secure execution using docker sandboxing
- Coding challenge management
- User Authentication
- Azure Blob Storage integration

## Running locally

First add `.env` file with your Azure credentials:

```
AzureBlob__ConnectionString=<KEY>
AzureBlob__ContainerName=<CONTAINER_NAME>
```

Then run:

```bash
docker compose up --build
```

The API will be available at:
```
http://localhost:8080
```

## License

Distributed under the MIT License. See `LICENSE` for more information.
