int RED_LED_PIN = 6;
int GREEN_LED_PIN = 5;

int VOLUME_PIN = A0;
int MOVEMENT_PIN = A1;

int BUTTON_PIN = 10;


void setup() {
  // put your setup code here, to run once:
  pinMode(RED_LED_PIN, OUTPUT);
  pinMode(GREEN_LED_PIN, OUTPUT);

  pinMode(VOLUME_PIN, INPUT);
  pinMode(MOVEMENT_PIN, INPUT);

  pinMode(BUTTON_PIN, INPUT_PULLUP);

  Serial.begin(115200);

  // Wait for a serial connection
  while (!Serial.availableForWrite());
}

void loop() {
  // put your main code here, to run repeatedly:

  // Volume
  writeSerialMessage("v", analogRead(VOLUME_PIN));

  // Movement
  writeSerialMessage("m", analogRead(MOVEMENT_PIN));

  // Answer
  writeSerialMessage("a", digitalRead(BUTTON_PIN));

  delay(100);
}


void writeSerialMessage(char* unicode, uint16_t value) {
  Serial.write(unicode);
  uint8_t message[2] = {static_cast<uint8_t>(value & 0xFF), static_cast<uint8_t>((value >> 8) & 0xFF)};
  //Serial.println(value);
  Serial.write(sizeof(message));
  Serial.write(message, sizeof(message));
}

// Handles incoming messages
// Called by Arduino if any serial data has been received
void serialEvent()
{
  char buffer[2];
  Serial.readBytes(buffer, 2);

  if (buffer[0] == 'l') {
    int size = buffer[1];

    // Read the value of red alpha
    byte firstBuffer[size];
    Serial.readBytes(firstBuffer, size);
    int redAlpha = 0;
    memcpy(&redAlpha, firstBuffer, size);

    // Read the value of green alpha
    byte secondBuffer[size];
    Serial.readBytes(secondBuffer, size);
    int greenAlpha = 0;
    memcpy(&greenAlpha, secondBuffer, size);

    analogWrite(RED_LED_PIN, redAlpha);
    analogWrite(GREEN_LED_PIN, greenAlpha);
  }
}
