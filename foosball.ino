//--------------------------------------------------------------------------
// Luke Robinson, Charlie Watkins, Seena Vafaee, Justin Joyce
//
// Code for haptic foosball interface
// Used in unison with the Unity game engine
//--------------------------------------------------------------------------

// Includes
#include <math.h>

//Serial commands
#include <SoftwareSerial.h>
#include <SerialCommand.h>
SerialCommand sCmd;


// Pin declarations for rotational joint (motor 1)
#define pwmPinRot 5  // PWM output pin for motor 1
#define dirPinRot 8  // direction output pin for motor 1
#define encPinRotA 2 // rotation encoder channel a
#define encPinRotB 5 // rotation encoder channel b

// Pin declarations for prismatic joint (motor 2)
#define pwmPinPris 6 // PWM output pin for motor 2
#define dirPinPris 7 // direction output pin for motor 2
#define encPinPrisA 3 // rotation encoder channel a
#define encPinPrisB 4 // rotation encoder channel b

volatile long encRotCount;
volatile long encPrisCount;
long last_rot = 0;
long last_pris = 0;
String command;
bool b1 = false;
bool b2 = false;


unsigned long prisTimeout;
unsigned long rotTimeout;
int rotSpeedGlobal = 0;
int prisSpeedGlobal = 0;
// --------------------------------------------------------------
// Setup function
// --------------------------------------------------------------
void setup()
{
  // Set up serial communication
  Serial.begin(9600);


  //this function changes the timers to 1000x faster
  // Set PWM frequency
  //setPwmFrequency(pwmPinRot, 1);
  //setPwmFrequency(pwmPinPris, 1);

  //Encoder inputs and interrupts
  pinMode(encPinRotA, INPUT_PULLUP);
  pinMode(encPinRotB, INPUT_PULLUP);
  pinMode(encPinPrisA, INPUT_PULLUP);
  pinMode(encPinPrisB, INPUT_PULLUP);
  attachInterrupt(digitalPinToInterrupt(encPinRotA), encRot, RISING);
  attachInterrupt(digitalPinToInterrupt(encPinPrisA), encPris, RISING);

  //Motor outputs
  pinMode(pwmPinRot, OUTPUT);
  pinMode(dirPinRot, OUTPUT);
  pinMode(pwmPinPris, OUTPUT);
  pinMode(dirPinPris, OUTPUT);

  //Initialize motor
  analogWrite(pwmPinRot, 0);     // set to not be spinning (0/255)
  digitalWrite(dirPinRot, LOW);  // set direction
  analogWrite(pwmPinPris, 0);    // set to not be spinning (0/255)
  digitalWrite(dirPinPris, LOW); // set direction


  //Setup callbacks for SerialCommand commands
  sCmd.addCommand("m", motors); //set motor speeds
  sCmd.addCommand("e", encoders); //get encoder values
  sCmd.addDefaultHandler(unrecognized);  // Handler for command that isn't matched  (says "What?")
  Serial.println("Haptic foosball ready");


}


// --------------------------------------------------------------
// Main Loop
// --------------------------------------------------------------
void loop()
{
  //read serial and process commands using callbacks
  sCmd.readSerial();

  if (encPrisCount > last_pris) {
    command = "W" + (String) encPrisCount;
    b1 = true;
  } else if (encPrisCount < last_pris) {
    command = "S" + (String) encPrisCount;
    b1 = true;
  } else {
    b1 = false;
  }

  if (encRotCount > last_rot) {
    Serial.println(" " + command);
  }  else if (b1) {
    Serial.println(command);
  }
  //update rate variables
  last_rot = encRotCount;
  last_pris = encPrisCount;


  if (prisTimeout > millis()) {
    analogWrite(pwmPinPris, prisSpeedGlobal); // output the signal
    analogWrite(pwmPinRot, rotSpeedGlobal); // output the signal

  }
  else {
    analogWrite(pwmPinPris, 0); // output the signal
    analogWrite(pwmPinRot, 0); // output the signal
  }
}

/* Encoders - send the encoder values over serial separated by a space
*/
void encoders() {
  Serial.print(encRotCount);
  Serial.print(" ");
  Serial.print(encPrisCount);
  Serial.println();
}


/* Motors - callback for serial command to parse motor speeds

*/
void motors()
{
  int rotSpeed;
  int prisSpeed;
  char *arg;
  arg = sCmd.next();
  if (arg != NULL)
  {

    rotSpeed = atoi(arg);  // Converts a char string to an integer
    if (rotSpeed < 0) {
      digitalWrite(dirPinRot, HIGH);  // set backwards
      rotSpeed = abs(rotSpeed);
    }
    else {
      digitalWrite(dirPinRot, LOW);  // set forwards
    }
    rotSpeedGlobal = constrain(rotSpeed, 0, 255);
    rotTimeout = millis() + 100;
    //    analogWrite(pwmPinRot, rotSpeed); // output the signal
    //    delay(200);
    //    analogWrite(pwmPinRot, 0); // output the signal


  }
  else {
    Serial.println("No arguments");
  }


  arg = sCmd.next();
  if (arg != NULL)
  {

    prisSpeed = atoi(arg);  // Converts a char string to an integer
    if (prisSpeed < 0) {
      digitalWrite(dirPinPris, HIGH);  // set backwards
      prisSpeed = abs(prisSpeed);
    }
    else {
      digitalWrite(dirPinPris, LOW);  // set forwards
    }

    prisSpeedGlobal = constrain(prisSpeed, 0, 255);
    prisTimeout = millis() + 200;
    //    analogWrite(pwmPinPris, prisSpeed); // output the signal
    //    delay(200);
    //    analogWrite(pwmPinPris, 0); // output the signal

  }
  else {
    Serial.println("No second argument");
  }

}

/*
    This gets set as the default handler, and gets called when no other command matches.
*/
void unrecognized()
{
  Serial.println("Error pasring command");
}


/*
   encoder function for rotation
*/
void encRot() {
  if (digitalRead(encPinRotB) == LOW) {
    encRotCount++;
  }
  else {
    encRotCount--;
  }
}

/*
   encoder function for translation
*/
void encPris() {
  if (digitalRead(encPinPrisB) == LOW) {
    encPrisCount++;
  }
  else {
    encPrisCount--;
  }
}
