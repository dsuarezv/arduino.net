#ifndef SOFT_DEBUGGER
#define SOFT_DEBUGGER

#include <Arduino.h>
#include <stdint.h>

#define SOFTDEBUGGER_CONNECT DbgConnect();
#define SOFTDEBUGGER_BREAK(a) DbgBreak(a);


#define DBG_HEADER_MAGIC         255
#define DBG_HEADER_CONNECT_TYPE  254
#define DBG_HEADER_BREAK_TYPE    253
#define DBG_HEADER_TRACEQUERY_TYPE    250
#define DBG_HEADER_TRACEANSWER_TYPE   249
#define DBG_HEADER_CONTINUE_TYPE      230

//static void DbgSaveRegisters() __attribute__ ((noinline));
static void DbgSendTrace(uint32_t address, uint8_t size);

void DbgBreak(uint8_t breakpointNo) __attribute__ ((noinline));


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




static void DbgConnect()
{
    Serial.begin(115200);
    
    DbgConnectPacket p;
    p.Header.Magic = DBG_HEADER_MAGIC;
    p.Header.Type = DBG_HEADER_CONNECT_TYPE;
    
    Serial.write((uint8_t*)&p, sizeof(DbgConnectPacket));
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
            uint32_t address;
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



/*
byte __DbgSavedRegisters[36];  // 32 registers + SP (16bits) + PC (16 bits)

static void DbgSaveRegisters()
{
    // Source:      regY
    // Destination: regZ (__DbgSavedRegisters). Z register (r30,r31) is lost.
    
    asm volatile ("\
        cli                     \n\t\
        push r29                \n\t\
        push r28                \n\t\
        push r27                \n\t\
        \
        ldi r28, 32             ; Source: Y = position 32      \n\t\
        ldi r29, 0              ; Copying down                 \n\t\
        \
    DbgSaveRegisters_loop:      \n\t\
        ld r27, -Y              ; buffer source content in r27                     \n\t\
        st Z+, r27              ; Store the buffer content in *Z and increment Z   \n\t\
        cpi r28, 0              \n\t\
        brne DbgSaveRegisters_loop  \n\t\
        \
        ; Copy the stack pointer  \n\t\
        \
        ldi r27, 0x3e           \n\t\
        st Z+, r27              \n\t\
        ldi r27, 0x3d           \n\t\
        st Z+, r27              \n\t\
        \
        pop r27                 \n\t\
        pop r28                 \n\t\
        pop r29                 \n\t\
        \
        ; Copy program counter from the stack         \n\t\
        pop r1                  ; PC                  \n\t\
        pop r2                  ; PC                  \n\t\
        st Z+, r1               \n\t\
        st Z+, r2               \n\t\
        push r2                 \n\t\
        push r1                 \n\t\
        \
        ; Recover r1 and r2 from the saved buffer     \n\t\
        \
        st -Z, r1               \n\t\
        st -Z, r1               \n\t\
        st -Z, r1               \n\t\
        st -Z, r1               \n\t\
        st -Z, r1               \n\t\
        st -Z, r2               \n\t\
        \
        sei                     \n\t\
        "
         :
         : "z" (__DbgSavedRegisters));
} 
*/


void DbgBreakImpl(uint8_t breakpointNo)
{
    DbgBreakPacket p;
    
    p.Header.Magic = DBG_HEADER_MAGIC;
    p.Header.Type = DBG_HEADER_BREAK_TYPE;
    p.BreakpointNumber = breakpointNo;
    
    Serial.write((uint8_t*)&p, sizeof(DbgBreakPacket));
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


#endif
