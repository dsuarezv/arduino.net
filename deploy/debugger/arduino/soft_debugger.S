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

	; Save status register before any other instruction touches it
   	in r27, 0x3f

	; Setup copy destination
	
	ldi	r30, lo8(__DbgSavedRegisters)		; r30, r31 (Z) = destination
   	ldi	r31, hi8(__DbgSavedRegisters)

   	
   	; 1. Copy status register

   	st Z+, r27

   	; 2. Copy registers r0 - r25

   	ldi r28, 0 			; r28, r29 (Y) = source
   	ldi r29, 0 			; register file starts at 0x0000

1:
	ld r27, Y+
	st Z+, r27
	cpi r28, 26         ; copy 26 registers (r0 - r25)
	brne 1b
	

	; 3. Copy rest of the registers (r26-r31) from the stack, and also the program counter


	in r28, 0x3d		; SPL
	in r29, 0x3e		; SPH
	adiw r28, 1         ; stackpointer points to the "next free slot", so our values start in the previous byte
	ldi r26, 0			; byte counter (bytes to copy)
2:
    ld r27, Y+
    st Z+, r27
    inc r26
    cpi r26, 8			; copy 6 registers in the stack + 2 for the program counter = 8. NOTE: in the mega, the PC is 3 bytes, we should always copy 3 instead of 2 and let the pc side handle the 3rd byte.
    brne 2b



    ; 4. Finally copy the stack pointer itself, removing the fingerprint of this method
	
	in r28, 0x3d		; SPL
	in r29, 0x3e		; SPH
	adiw r28, 8			; 6 bytes in this func stack + 2 for the program counter
	st Z+, r28
	st Z+, r29


	; TODO: Restore SREG


	pop r26
	pop r27
	pop r28
	pop r29
	pop r30
	pop r31

	sei
	ret

.end

