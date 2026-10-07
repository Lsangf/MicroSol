//#include <Arduino.h>
//#include <WiFi.h>
//#include <WebServer.h>
//
//const char* ssid = "-";
//const char* password = "-";
//
//
//const int LED_GREEN = 18;
//
//WebServer server(80);
//
//void handleGreenLed()
//{
//    digitalWrite(LED_GREEN, HIGH);
//
//    server.send(200,"text/plain", "Green LED is ON");
//}
//
//void setup()
//{
//    Serial.begin(115200);
//
//    pinMode(LED_GREEN, OUTPUT);
//    digitalWrite(LED_GREEN, LOW);
//
//    WiFi.softAP(ssid, password);
//
//    Serial.println("ESP32 Wi-Fi started");
//    Serial.print("IP address: ");
//    Serial.println(WiFi.softAPIP());
//
//    server.on("/led/green/on", handleGreenLed);
//
//    server.begin();
//
//    Serial.println("HTTP server started");
//}
//
//void loop()
//{
//    server.handleClient();
//}

#include <Arduino.h>
#include <WiFi.h>
#include <WebServer.h>

const char* ssid = "-";
const char* password = "-";

const byte LED_GREEN_PIN = 18;
const byte LED_RED_PIN = 19;
const byte LED_YELLOW_PIN = 21;

WebServer server(80);

void handleLed()
{
    if (server.uri() == "/led/green/on")
    {
        digitalWrite(LED_GREEN_PIN, HIGH);
        server.send(200, "text/plain", "Green LED is ON");
        Serial.print("Green LED is ON");
    }
    else if (server.uri() == "/led/red/on")
    {
        digitalWrite(LED_RED_PIN, HIGH);
        server.send(200, "text/plain", "Red LED is ON");
        Serial.print("Red LED is ON");
    }
    else if (server.uri() == "/led/yellow/on")
    {
        digitalWrite(LED_YELLOW_PIN, HIGH);
        server.send(200, "text/plain", "Yellow LED is ON");
		Serial.print("Yellow LED is ON");
    }
    else
    {
        server.send(404, "text/plain", "Not Found");
        Serial.print("Not Found");
    }
}

void setup()
{
    Serial.begin(115200);

    pinMode(LED_GREEN_PIN, OUTPUT);
	pinMode(LED_RED_PIN, OUTPUT);
	pinMode(LED_YELLOW_PIN, OUTPUT);

    digitalWrite(LED_GREEN_PIN, LOW);
    digitalWrite(LED_RED_PIN, LOW);
    digitalWrite(LED_YELLOW_PIN, LOW);

    WiFi.begin(ssid, password);

    Serial.print("Connecting to Wi-Fi");

    while (WiFi.status() != WL_CONNECTED)
    {
        delay(500);
        Serial.print(".");
    }

    Serial.println();
    Serial.println("ESP32 Wi-Fi connected");

    Serial.print("IP address: ");
    Serial.println(WiFi.localIP());

    server.on("/led/green/on", handleLed);
    server.on("/led/red/on", handleLed);
    server.on("/led/yellow/on", handleLed);

    server.begin();

    Serial.println("HTTP server started");
}

void loop()
{
    server.handleClient();
}