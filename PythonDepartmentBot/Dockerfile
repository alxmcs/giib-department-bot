FROM python:3

WORKDIR /app 

RUN pip3 install --no-cache-dir discord.py

COPY Bot.py config.json links.json schedule.json ./

ENTRYPOINT python3 Bot.py
