# This is a docker image with preinstalled pytest

FROM python:3.11-slim

RUN pip install --no-cache-dir pytest pytest-timeout

WORKDIR /app

RUN useradd -m tester
USER tester
