# Pulse Alert IoT

## 1) Funcionamento e uso
O **Pulse Alert** é um sistema de monitoramento de pulsação em tempo real baseado em IoT. O protótipo utiliza o microcontrolador ESP32 DevKit V1 conectado a um sensor biomédico (Pulse Sensor Amped) para detectar batimentos cardíacos. Os dados são processados e comparados com parâmetros pré-definidos. Em caso de anomalia, o sistema aciona um motor de vibração tipo moeda como alerta tátil ao usuário e transmite as informações via protocolo MQTT para um broker na nuvem (EMQX Cloud no caso). Um cliente remoto (aplicação em .Net) assina os tópicos MQTT e exibe os dados em tempo real.  
Para reproduzir o projeto, basta clonar este repositório, carregar o código no ESP32 e configurar o broker MQTT conforme indicado.

---

## 2) Software desenvolvido e documentação de código
O repositório contém:
- Código-fonte para o **ESP32**, responsável pela leitura do sensor, acionamento do atuador e publicação dos dados via MQTT.  
- Código-fonte para o **client .Net**, que assina os tópicos MQTT e exibe os valores recebidos.  
- Documentação inline nos arquivos, explicando cada função e módulo.  
- Exemplos de configuração de tópicos MQTT:  
  - `esp32/pulse` → estado (NORMAL/ALERTA)  
  - `esp32/pulse/valor` → valores brutos do sensor, esse é utilizado nos testes.  

---

## 3) Descrição do hardware utilizado
- **Plataforma de desenvolvimento:** ESP32 DevKit V1.  
- **Sensor biomédico real:** Pulse Sensor Amped (sensor óptico de batimentos cardíacos).  
- **Atuador real:** Motor de vibração tipo moeda (coin vibration motor).  
- **Simulação Wokwi:**  
  - Potenciômetro em substituição ao Pulse Sensor.  
  - LED em substituição ao motor de vibração.  
- **Outros componentes:** resistores de 220 Ω para proteção do LED na simulação.   

---

## 4) Documentação das interfaces, protocolos e módulos de comunicação
- **Interface de sensores:** leitura analógica do Pulse Sensor Amped pelo ESP32.  
- **Interface de atuadores:** acionamento digital do motor de vibração (ou LED na simulação).  
- **Protocolo de comunicação:**  
  - **Wi-Fi (TCP/IP):** conexão do ESP32 à rede local.  
  - **MQTT:** protocolo leve de mensagens para IoT.  
    - Broker utilizado: **EMQX Cloud**.  
    - Tópicos definidos:  
      - `esp32/pulse` → estado do sistema.  
      - `esp32/pulse/valor` → valores de pulsação.  
- **Cliente remoto:** aplicação em C# que assina os tópicos e exibe os dados recebidos.  

---
