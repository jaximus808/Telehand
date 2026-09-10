//Uno script

#include<Servo.h>

const byte numChars = 32;
char receivedChars[numChars];   // an array to store the received data

boolean newData = false;

int dataNumber = 0;    
bool reading = false; 

int pos = 0; 

int motorPos = 0; 

Servo pointer; 
Servo middle; 
Servo ring;


void setup() {
  Serial.begin(115200);
  pointer.attach(10);
  middle.attach(9);
  ring.attach(11);
  
  while(!Serial)
  {
    ;
  }
  Serial.println("connected");
//  servos[0].attach(9);
//  servos[1].attach(10);
//  servos[2].attach(11);
}



void loop() {
  while(Serial.available())
  {
    
    char c = Serial.read();
    Serial.println(c);
    if(c == '<')
    {
      reading = true; 
    }
    else if(c=='>' && reading)
    {
      int index = 0; 
      int rotBuffer[3]; 
      reading = false; 
      String stringInt = "";
      for(int i = 0; i < pos; i++)
      {
        //Serial.write(receivedChars[i]);
        if(receivedChars[i] == ',')
        {
          int rotation = stringInt.toInt();
          
          if(rotation < 0) rotation = 0; 
          if(rotation > 360) rotation = 360; 

          rotBuffer[index] = rotation/2;
          index++; 
          Serial.println(rotation);
          stringInt = "";
          //serv
        }
        else stringInt += receivedChars[i];
        
      }
      ServoHandler(rotBuffer);
      pos = 0; 
      motorPos = 0; 
      Serial.println();
    }
    else if(reading)
    {
      receivedChars[pos] = c; 
      pos++;
      if(pos == 32)
      {
        pos == 31;
      }
    }
  }
}

void ServoHandler(int* rots)
{
  Serial.println("writing to pointer");
  Serial.println(rots[0]);
  
  pointer.write(180 - rots[0]);
  
  Serial.println("writing to middle");
  Serial.println(rots[1]);
  
  middle.write(180 - rots[1]);
  
  Serial.println("writing to ring");
  Serial.println(rots[2]);
  
  ring.write(rots[2]);
}
