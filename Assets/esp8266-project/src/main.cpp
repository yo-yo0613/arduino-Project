#include <Arduino.h>
#include <ESP8266WiFi.h>

// ======== Wi-Fi 設定 ========
const char* ssid     = "71H3F-2.4G";
const char* password = "0229914954";

// NodeMCU 腳位：D2=GPIO4, D5=GPIO14, D6=GPIO12
const int ButtonPin1 = D2;
const int ButtonPin2 = D5;
const int ButtonPin3 = D6;

// 建立 TCP Server，隨便選個 port，例如 4210
WiFiServer server(4210);
WiFiClient client;

int lastB1 = HIGH;
int lastB2 = HIGH;
int lastB3 = HIGH;

void setup() {
    Serial.begin(9600);
    delay(1000);
    Serial.println();
    Serial.println("Hello, Arduino with WiFi!");

    // 按鈕用內建上拉電阻，按下時會讀到 LOW
    pinMode(ButtonPin1, INPUT_PULLUP);  // INPUT_PULLUP，按下 = 0（LOW），沒按 = 1（HIGH）。
    pinMode(ButtonPin2, INPUT_PULLUP);
    pinMode(ButtonPin3, INPUT_PULLUP);

    // ======== Wi-Fi 連線 ========
    WiFi.mode(WIFI_STA);
    WiFi.begin(ssid, password);
    Serial.print("Connecting to WiFi");
    while (WiFi.status() != WL_CONNECTED) {
        delay(500);
        Serial.print(".");
    }
    Serial.println();
    Serial.print("Connected! IP address: ");
    Serial.println(WiFi.localIP()); // Unity 裡要填這個 IP

    // 啟動 TCP Server
    server.begin();
    Serial.println("TCP server started on port 4210");
}

void sendButtonState(int buttonD2, int buttonD5, int buttonD6) {
    if (!client || !client.connected()) return;

    // pressed = 1 / not pressed = 0（用「0=按下」還是「1=按下」你自己選）
    int p1 = (buttonD2 == LOW) ? 1 : 0;
    int p2 = (buttonD5 == LOW) ? 1 : 0;
    int p3 = (buttonD6 == LOW) ? 1 : 0;

    // 判斷現在「主要被按下」的是哪一顆（簡單版：第一顆被偵測到就算）
    String currentBtn = "None";
    if (p1) currentBtn = "Button1";
    else if (p2) currentBtn = "Button2";
    else if (p3) currentBtn = "Button3";

    // 傳一行文字給 Unity，格式你可以自訂
    // 例如：B1=1,B2=0,B3=0,CUR=Button1
    String msg = "ButtonD2=" + String(p1) +
                 ",ButtonD5=" + String(p2) +
                 ",ButtonD6=" + String(p3) +
                 ",CurrentButton=" + currentBtn + "\n";

    client.print(msg);
    Serial.print("Sent: " + msg);
}

void loop() {

    // 如果目前還沒有 client 連進來，就等待 Unity 連線
    if (!client || !client.connected()) {
        client = server.available(); // 等待新 client
        return;
    }

    int buttonStateD2 = digitalRead(ButtonPin1);
    int buttonStateD5 = digitalRead(ButtonPin2);
    int buttonStateD6 = digitalRead(ButtonPin3);

    // 只有狀態有變時才送（避免刷爆）
    if (buttonStateD2 != lastB1 || 
        buttonStateD5 != lastB2 || 
        buttonStateD6 != lastB3
    ) 
    {
        sendButtonState(buttonStateD2, buttonStateD5, buttonStateD6);
        lastB1 = buttonStateD2;
        lastB2 = buttonStateD5;
        lastB3 = buttonStateD6;
    }    

    delay(20);  // 簡單 debounce & 降低頻率
}