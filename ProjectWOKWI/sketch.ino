#include <WiFi.h>
#include <PubSubClient.h>

const char* ssid = "Wokwi-GUEST";
const char* password = "";

const char* mqtt_server = "o801170c.ala.dedicated.aws.emqxcloud.com";
const int mqtt_port = 1883;
const char* mqtt_user = "lucas_iot";
const char* mqtt_pass = "Trocar#123Trocar#123";

WiFiClient espClient;
PubSubClient client(espClient);

#define PULSE_PIN 34  // Potenciômetro simulando o PulseSensor
#define VIB_PIN   15  // LED simulando vibração
int limite = 2000;

void callback(char* topic, byte* message, unsigned int length) {
  String msg;
  for (unsigned int i = 0; i < length; i++) {
    msg += (char)message[i];
  }
  Serial.print("Mensagem recebida: ");
  Serial.print(topic);
  Serial.print("\n");
  Serial.println(msg);

  if (msg == "liga") {
    digitalWrite(VIB_PIN, HIGH);
    Serial.println("LED acionado via MQTT");
  } else if (msg == "desliga") {
    digitalWrite(VIB_PIN, LOW);
    Serial.println("LED desligado via MQTT");
  }
}

void reconnect() {
  while (!client.connected()) {
    Serial.print("Conectando ao MQTT...");
    if (client.connect("ESP32ClientLucas", mqtt_user, mqtt_pass)) {
      Serial.println("Conectado!");
      client.subscribe("esp32/comandos"); // Tópico para receber comandos
    } else {
      Serial.print("Falhou, erro: ");
      Serial.print(client.state());
      delay(2000);
    }
  }
}

void setup() {
  Serial.begin(115200);
  pinMode(VIB_PIN, OUTPUT);

  WiFi.begin(ssid, password);
  Serial.print("Conectando ao WiFi");
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("\nWiFi conectado!");

  client.setServer(mqtt_server, mqtt_port);
  client.setCallback(callback);

  Serial.println("Pulse Alert (Simulação Wokwi + EMQX Cloud)");
  Serial.println("Gire o potenciômetro para simular batimentos...");
}

void loop() {
  if (!client.connected()) {
    reconnect();
  }
  client.loop();

  int valor = analogRead(PULSE_PIN);

  // Exibir o valor dessa bagaça no Serial Monitor
  Serial.print("Valor lido: ");
  Serial.println(valor);

  char msg[50];
  sprintf(msg, "{\"valor\":%d}", valor);
  client.publish("esp32/pulse/valor", msg);

  if (valor > limite) {
    digitalWrite(VIB_PIN, HIGH);
    Serial.println("ALERTA (LED ON)");
    client.publish("esp32/pulse", "ALERTA");   // Publica no tópico
  } else {
    digitalWrite(VIB_PIN, LOW);
    Serial.println("NORMAL (LED OFF)");
    client.publish("esp32/pulse", "NORMAL");   // Publica no tópico
  }

  delay(200);
}
