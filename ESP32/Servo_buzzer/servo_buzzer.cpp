#include <Arduino.h>
#include <ESP32Servo.h>

#define LED_PIN1 18
#define LED_PIN2 19

#define BUTTON_PIN1 4
#define BUTTON_PIN2 16

#define BUZZER_PIN 22
#define SERVO_PIN 21

Servo servo;

bool lastButtonState1 = HIGH;
bool lastButtonState2 = HIGH;

int servoStop = 90;
int servoLeft = 80;
int servoRight = 100;

unsigned long movementStartTime = 0;
unsigned long movementTime = 0;

bool movingLeft = false;
bool movingRight = false;


unsigned long buzzerStartTime = 0;

int buzzerBeeps = 0;
int buzzerCurrentBeep = 0;

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


void setup()
{
    Serial.begin(115200);

    pinMode(BUTTON_PIN1, INPUT_PULLUP);
    pinMode(BUTTON_PIN2, INPUT_PULLUP);

    pinMode(LED_PIN1, OUTPUT);
    pinMode(LED_PIN2, OUTPUT);

    pinMode(BUZZER_PIN, OUTPUT);

    digitalWrite(LED_PIN1, LOW);
    digitalWrite(LED_PIN2, LOW);
    digitalWrite(BUZZER_PIN, LOW);

    servo.attach(SERVO_PIN);

    servo.write(servoStop);

    Serial.println("ESP32 launched");
    Serial.println("System is ready");
}

void loop()
{
    bool buttonState1 = digitalRead(BUTTON_PIN1);
    bool buttonState2 = digitalRead(BUTTON_PIN2);


    if (buttonState1 != lastButtonState1)
    {
        if (buttonState1 == LOW)
        {
            Serial.println("Left: pressed");

            digitalWrite(LED_PIN1, HIGH);

            startBuzzer(2);

            movementStartTime = millis();
            movingLeft = true;
            movingRight = false;
        }
        else
        {
            Serial.println("Left: released");

            digitalWrite(LED_PIN1, LOW);

            servo.write(servoStop);

            if (movingLeft)
            {
                movementTime = millis() - movementStartTime;
            }

            movingLeft = false;
        }

        lastButtonState1 = buttonState1;
    }


    if (buttonState2 != lastButtonState2)
    {
        if (buttonState2 == LOW)
        {
            Serial.println("Right: pressed");

            digitalWrite(LED_PIN2, HIGH);

            startBuzzer(3);

            movementStartTime = millis();
            movingRight = true;
            movingLeft = false;
        }
        else
        {
            Serial.println("Right: released");

            digitalWrite(LED_PIN2, LOW);

            servo.write(servoStop);

            if (movingRight)
            {
                movementTime = millis() - movementStartTime;
            }

            movingRight = false;
        }

        lastButtonState2 = buttonState2;
    }


    if (buttonState1 == LOW && buttonState2 == LOW)
    {
        Serial.println("Both buttons pressed");

        if (movingLeft)
        {
            servo.write(servoRight);

            if (millis() - movementStartTime >= movementTime)
            {
                servo.write(servoStop);

                movingLeft = false;
            }
        }

        else if (movingRight)
        {
            servo.write(servoLeft);

            if (millis() - movementStartTime >= movementTime)
            {
                servo.write(servoStop);

                movingRight = false;
            }
        }
    }

    else
    {
        if (buttonState1 == LOW)
        {
            servo.write(servoLeft);
        }
        else if (buttonState2 == LOW)
        {
            servo.write(servoRight);
        }
        else
        {
            servo.write(servoStop);
        }
    }


    updateBuzzer();
}