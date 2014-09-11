#include "soft_debugger.h"
#include <avr/io.h>

.section .text

.global DbgSaveRegisters



DbgSaveRegisters:
	cli
	push r31
	push r30
	push r29
	push r28
	push r27
	push r26



	; Setup copy destination
	
	ldi	r30, lo8(__DbgSavedRegisters)
   	ldi	r31, hi8(__DbgSavedRegisters)

   	; 1. Copy registers r0 - r25

   	ldi r28, 0 			
   	ldi r29, 0 			; register file starts at 0x0000

.copyLoop:
	ld r27, Y+
	st Z+, r27
	cpi r28, 26         ; copy 26 registers (r0 - r25)
	brne .copyLoop
	

	; 2. Copy rest of the registers (r27-r31) from the stack, and also the program counter


	in r28, 0x3d		; SPL
	in r29, 0x3e		; SPH
	adiw r28, 1         ; stackpointer points to the "next free slot", so our values start in the previous byte
	ldi r26, 0			; byte counter (bytes to copy)
.stackCopyLoop:
    ld r27, Y+
    st Z+, r27
    inc r26
    cpi r26, 8			; copy 6 registers in the stack + 2 for the program counter = 8
    brne .stackCopyLoop

    ; 3. Finally copy the stack pointer itself, removing the fingerprint of this method
	
	in r28, 0x3d		; SPL
	in r29, 0x3e		; SPH
	adiw r28, 8			; 6 bytes in this func stack + 2 for the program counter
	st Z+, r28
	st Z+, r29


	pop r26
	pop r27
	pop r28
	pop r29
	pop r30
	pop r31

	sei
	ret

.end


/*
TestSaveRegisters:
	cli
	push r31
	push r30
	push r29
	push r28
	push r27
	
	; REMOVE ME
	push r26
	push r25
	push r24
	push r23
	push r22
	push r21
	push r20
	
	push r19
	push r18
	push r17
	push r16
	push r15
	push r14
	push r13
	push r12
	push r11
	push r10

	push r9
	push r8
	push r7
	push r6
	push r5
	push r4
	push r3
	push r2
	push r1
	push r0

	; REMOVE ME
	ldi r16, 100
	mov r0, r16
	ldi r16, 101
	mov r1, r16
	ldi r16, 102
	mov r2, r16
	ldi r16, 103
	mov r3, r16
	ldi r16, 104
	mov r4, r16
	ldi r16, 105
	mov r5, r16
	ldi r16, 106
	mov r6, r16
	ldi r16, 107
	mov r7, r16
	ldi r16, 108
	mov r8, r16
	ldi r16, 109
	mov r9, r16
	ldi r16, 110
	mov r10, r16
	ldi r16, 111
	mov r11, r16
	ldi r16, 112
	mov r12, r16
	ldi r16, 113
	mov r13, r16
	ldi r16, 114
	mov r14, r16
	ldi r16, 115
	mov r15, r16


	ldi r16, 116
	ldi r17, 117
	ldi r18, 118
	ldi r19, 119
	ldi r20, 120
	ldi r21, 121
	ldi r22, 122
	ldi r23, 123
	ldi r24, 124
	ldi r25, 125
	ldi r26, 126
	ldi r27, 127
	; REMOVE ME

	; REMOVE ME

	; Setup copy
	ldi	r30, lo8(__DbgSavedRegisters)
   	ldi	r31, hi8(__DbgSavedRegisters)
   	ldi r28, 32
   	ldi r29, 0

.loop:
	ld r16, -Y
	st Z+, r16
	cpi r28, 0
	brne .loop
	

	; Stack pointer
	in r28, 0x3d		; SPL
	in r29, 0x3e		; SPH
	st Z+, r28
	st Z+, r29




	; REMOVE ME
	;pop r0
	;pop r1
	;pop r2
	pop r3
	pop r4
	pop r5
	pop r6
	pop r7
	pop r8
	pop r9

	pop r10
	pop r11
	pop r12
	pop r13
	pop r14
	pop r15
	pop r16
	pop r17
	pop r18
	pop r19

	pop r20
	pop r21
	pop r22
	pop r23
	pop r24
	pop r25
	pop r26
	; REMOVE ME	


	pop r27
	pop r28
	pop r29
	pop r30
	pop r31

	sei
	ret
*/



