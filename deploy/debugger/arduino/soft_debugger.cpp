#include "soft_debugger.h"
#include "HardwareSerial.h"

#define DBG_HEADER_MAGIC         255
#define DBG_HEADER_CONNECT_TYPE  254
#define DBG_HEADER_BREAK_TYPE    253
#define DBG_HEADER_TRACEQUERY_TYPE    250
#define DBG_HEADER_TRACEANSWER_TYPE   249
#define DBG_HEADER_CONTINUE_TYPE      230

#define SAVED_REGS_SIZE 38

uint8_t __DbgSavedRegisters[SAVED_REGS_SIZE];


typedef struct 
{
    uint8_t Magic;
    uint8_t Type;
} DbgHeader;

typedef struct 
{
    DbgHeader Header;
} DbgConnectPacket;

typedef struct
{
    DbgHeader Header;
    uint8_t   BreakpointNumber;
} DbgBreakPacket;

typedef struct 
{
    DbgHeader Header;
    uint32_t  Address;
    uint8_t   Size;
} DbgTraceQueryPacket;

typedef struct 
{
    DbgHeader Header;
    uint8_t   Size;
} DbgTraceAnswerPacket;



static void DbgSendTrace(uint32_t address, uint8_t size);
static void DbgLoop();


void DbgConnect()
{
    Serial.begin(115200);
    
    DbgConnectPacket p;
    p.Header.Magic = DBG_HEADER_MAGIC;
    p.Header.Type = DBG_HEADER_CONNECT_TYPE;
    
    Serial.write((uint8_t*)&p, sizeof(DbgConnectPacket));

    DbgLoop();  // Wait for host to connect
}



static void DbgLoop()
{
    // Wait for query packets
    while(1)
    {
        if (Serial.available() < 2) continue;
        int magic = Serial.read();
        int type = Serial.read();
        if (magic != DBG_HEADER_MAGIC) continue;
        
        if (type == DBG_HEADER_TRACEQUERY_TYPE)
        {
            uint16_t address;
            uint8_t size;
            Serial.readBytes((char*)&address, sizeof(uint32_t));
            Serial.readBytes((char*)&size, sizeof(uint8_t));
            
            DbgSendTrace(address, size);
        }
        
        if (type == DBG_HEADER_CONTINUE_TYPE)
        {
            return;
        }
    }
}

void DbgBreak(uint8_t breakpointNo)
{
    DbgBreakPacket p;
    
    p.Header.Magic = DBG_HEADER_MAGIC;
    p.Header.Type = DBG_HEADER_BREAK_TYPE;
    p.BreakpointNumber = breakpointNo;
    
    Serial.write((uint8_t*)&p, sizeof(DbgBreakPacket));
    Serial.write(__DbgSavedRegisters, SAVED_REGS_SIZE);
    DbgLoop();
}

static void DbgSendTrace(uint32_t address, uint8_t size)
{
    DbgTraceAnswerPacket p;
    
    p.Header.Magic = DBG_HEADER_MAGIC;
    p.Header.Type = DBG_HEADER_TRACEANSWER_TYPE;
    p.Size = size;
    
    Serial.write((uint8_t*)&p, sizeof(DbgTraceAnswerPacket));
    Serial.write((uint8_t*)address, size);
}

