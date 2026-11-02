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
#include <ESP32Servo.h>

const char* ssid = "-";
const char* password = "-";

const byte LED_GREEN_PIN = 18;
const byte LED_YELLOW_PIN = 19;
const byte LED_RED_PIN = 21;

const byte BUZZER_PIN = 22;
const byte SERVO_PIN = 23;

bool greenOn = false;
bool yellowOn = false;
bool redOn = false;

Servo servo;

bool lastButtonState1 = HIGH;
bool lastButtonState2 = HIGH;

byte servoStop = 90;
byte servoLeft = 80;
byte servoRight = 100;

unsigned long movementStopTime = 0;
unsigned long movementTime = 0;
unsigned long movementTimeLeft = 0;
unsigned long movementTimeRight = 0;

bool movingLeft = false;
bool movingRight = false;

unsigned long buzzerStartTime = 0;

byte buzzerBeeps = 0;
byte buzzerCurrentBeep = 0;

bool buzzerActive = false;
bool buzzerState = false;

void startBuzzer(int count)
{
    buzzerBeeps = count;
    buzzerCurrentBeep = 0;

    buzzerActive = true;
    buzzerState = true;

    buzzerStartTime = millis();

    digitalWrite(BUZZER_PIN, HIGH);
}


void updateBuzzer()
{
    if (!buzzerActive)
        return;

    unsigned long currentTime = millis();

    if (currentTime - buzzerStartTime >= 150)
    {
        buzzerStartTime = currentTime;

        if (buzzerState)
        {
            digitalWrite(BUZZER_PIN, LOW);
            buzzerState = false;

            buzzerCurrentBeep++;

            if (buzzerCurrentBeep >= buzzerBeeps)
            {
                buzzerActive = false;
            }
        }
        else
        {
            if (buzzerCurrentBeep < buzzerBeeps)
            {
                digitalWrite(BUZZER_PIN, HIGH);
                buzzerState = true;
            }
        }
    }
}

WebServer server(80);

void handleLed()
{
	String uri = server.uri();

	if (uri == "/led/green/toggle")
	{
		greenOn = !greenOn;
		digitalWrite(LED_GREEN_PIN, greenOn ? HIGH : LOW);
		server.send(200, "text/plain", "Green LED is " + String(greenOn ? "ON" : "OFF"));
		Serial.println("Green LED is " + String(greenOn ? "ON" : "OFF"));
	}
	else if (uri == "/led/red/toggle")
	{
		redOn = !redOn;
		digitalWrite(LED_RED_PIN, redOn ? HIGH : LOW);
		server.send(200, "text/plain", "Red LED is " + String(redOn ? "ON" : "OFF"));
		Serial.println("Red LED is " + String(redOn ? "ON" : "OFF"));
	}
	else if (uri == "/led/yellow/toggle")
	{
		yellowOn = !yellowOn;
		digitalWrite(LED_YELLOW_PIN, yellowOn ? HIGH : LOW);
		server.send(200, "text/plain", "Yellow LED is " + String(yellowOn ? "ON" : "OFF"));
		Serial.println("Yellow LED is " + String(yellowOn ? "ON" : "OFF"));
	}
	else if (uri == "/servo/start")
	{
		byte randomPosition = random(servoLeft, servoRight);
		if (randomPosition == servoLeft) 
		{
			startBuzzer(1);
			movementTimeLeft = millis();
			movingLeft = !movingLeft;
			movingRight = !movingLeft;
			servo.write(randomPosition);

			server.send(200, "text/plain", "Servo started left");
			Serial.println("Servo started left");
		}
		else 
		{
			startBuzzer(1);
			movementTimeRight = millis();
			movingRight = !movingRight;
			movingLeft = !movingRight;
			servo.write(randomPosition);

			server.send(200, "text/plain", "Servo started right");
			Serial.println("Servo started right");
		}
	}
	else if (uri == "/servo/stop")
	{
		startBuzzer(3);
		movingLeft = false;
		movingRight = false;
		servo.write(servoStop);
		server.send(200, "text/plain", "Servo stopped");
		Serial.println("Servo stopped");
	}
	else if (uri == "/servo/reset")
	{
		if (movementTimeLeft > movementTimeRight)
		{
			movementTime = movementTimeLeft;
			movingRight = true;
			movingLeft = false;
		}
		else
		{
			movementTime = movementTimeRight;
			movingLeft = true;
			movingRight = false;
		}
		startBuzzer(3);

		servo.write(servoStop);
		server.send(200, "text/plain", "Servo reset");
		Serial.println("Servo reset");
	}
	else if (uri == "/servo/left25")
	{
		startBuzzer(1);
		movementTimeLeft = millis();
		movingLeft = !movingLeft;
		movingRight = !movingLeft;
		servo.write(servoLeft);
		server.send(200, "text/plain", "Servo turned left by 25 degrees");
		Serial.println("Servo turned left by 25 degrees");
	}
	else if (uri == "/servo/right25")
	{
		startBuzzer(1);
		movementTimeRight = millis();
		movingRight = !movingRight;
		movingLeft = !movingRight;
		servo.write(servoRight);
		server.send(200, "text/plain", "Servo turned right by 25 degrees");
		Serial.println("Servo turned right by 25 degrees");
	}
	else
	{
		server.send(404, "text/plain", "Not Found");
		Serial.println("Not Found");
	}


}

void setup()
{
    Serial.begin(115200);

    pinMode(LED_GREEN_PIN, OUTPUT);
	pinMode(LED_RED_PIN, OUTPUT);
	pinMode(LED_YELLOW_PIN, OUTPUT);
    pinMode(BUZZER_PIN, OUTPUT);

    digitalWrite(LED_GREEN_PIN, LOW);
    digitalWrite(LED_RED_PIN, LOW);
    digitalWrite(LED_YELLOW_PIN, LOW);
    digitalWrite(BUZZER_PIN, LOW);

    servo.attach(SERVO_PIN);
    servo.write(servoStop);

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

    server.on("/led/green/toggle", handleLed);
    server.on("/led/red/toggle", handleLed);
    server.on("/led/yellow/toggle", handleLed);
	server.on("/servo/start", handleLed);
	server.on("/servo/stop", handleLed);
	server.on("/servo/left25", handleLed);
	server.on("/servo/right25", handleLed);

    server.begin();

    Serial.println("HTTP server started");
}

void loop()
{
    server.handleClient();

	//if (millis() - movementTime < movementTime)
	//{
	//	if (movingLeft)
	//	{
	//		servo.write(servoRight);
	//	}
	//	else if (movingRight)
	//	{
	//		servo.write(servoLeft);
	//	}
	//}

	updateBuzzer();
}