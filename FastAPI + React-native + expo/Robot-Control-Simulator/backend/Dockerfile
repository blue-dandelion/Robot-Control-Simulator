FROM python:3.12

WORKDIR /Robot-Control-Simulator/backend

COPY requirements.txt .
COPY ./app ./app

RUN pip install -r requirements.txt

CMD [ "python", "./app/main.py" ]