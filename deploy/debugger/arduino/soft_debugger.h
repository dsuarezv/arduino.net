#ifndef SOFT_DEBUGGER
#define SOFT_DEBUGGER

#include <Arduino.h>
#include <stdint.h>

#define SOFTDEBUGGER_CONNECT DbgConnect();
#define SOFTDEBUGGER_BREAK(a) DbgSaveRegisters(); DbgBreak(a);


void DbgConnect();
void DbgSaveRegisters() __attribute__ ((noinline));
void DbgBreak(uint8_t breakpointNo) __attribute__ ((noinline));

#endif
