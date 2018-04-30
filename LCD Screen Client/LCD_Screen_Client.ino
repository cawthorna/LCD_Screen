#include <Wire.h>
#include <LiquidCrystal.h>
#include <TKLCD.h>
#include <TinkerKit.h>

//TKLCD_Serial lcd = TKLCD_Serial(); // when connecting to TKLCD over Serial
TKLCD_Local lcd = TKLCD_Local(); // when programming a TKLCD module itself

/* Use a variable called byteRead to temporarily store
   the data coming from the computer */
byte byteRead;
byte hPos = 0;
byte vPos = 0;

void setup() {                
// Set default brightness and contrast
brightness = slide.read();
contrast = map(pot.read(),0,1023,175,255);

// Turn the Serial Protocol ON
  Serial.begin(9600);
  lcd.begin();
  lcd.print("CPU   %   C|   %");
  lcd.setCursor(0,1);
  lcd.print("GPU   %   C|   %");
  //lcd.blink();
  lcd.noCursor();
  lcd.noBlink();
}

void loop() {
   /*  check if data has been sent from the computer: */
   if(Serial.available()) {
    char cmd = (char) Serial.read();
    if(cmd == 'p') {    
      //Write provided values
      //Write CPU  
      lcd.setCursor(3,0);
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.setCursor(7,0);
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
    
      //Write RAM
      lcd.setCursor(12,0);
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());

      //Write GPU
      lcd.setCursor(3,1);
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.setCursor(7,1);
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());

      //Write GPU RAM
      lcd.setCursor(12,1);
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
      lcd.print((char) Serial.read());
    }
    else if(cmd == 'c') {
      //Clear the screen
      //Write CPU  
      lcd.setCursor(3,0);
      lcd.print("   ");
      lcd.setCursor(7,0);
      lcd.print("   ");
    
      //Write RAM
      lcd.setCursor(12,0);
      lcd.print("   ");

      //Write GPU
      lcd.setCursor(3,1);
      lcd.print("   ");
      lcd.setCursor(7,1);
      lcd.print("   ");

      //Write GPU Ram
      lcd.setCursor(12,1);
      lcd.print("   ");
    }
    else if(cmd == 's') {
      //Sleep
      lcd.setContrast(0);
      lcd.setBrightness(0);
    }
    else if(cmd == 'w') {
      //Wake
      lcd.setBrightness(255);
      lcd.setContrast(230);
    }
  }
  //delay after checking.
  delay(100);
}


