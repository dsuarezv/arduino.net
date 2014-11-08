#ifndef SOFT_DEBUGGER
#define SOFT_DEBUGGER

#include <Arduino.h>
#include <stdint.h>

#define SOFTDEBUGGER_CONNECT DbgConnect();
#define SOFTDEBUGGER_BREAK(a) DbgSaveRegisters(); DbgBreak(a);

#ifdef __cplusplus
extern "C" {
#endif



void DbgConnect();
void DbgBreak(uint8_t breakpointNo) __attribute__ ((noinline));
void DbgCaptureValue(uint8_t captureId, uint32_t value) __attribute__ ((noinline));
extern void DbgSaveRegisters() __attribute__ ((noinline));


#ifdef __cplusplus
}
#endif

#endif
