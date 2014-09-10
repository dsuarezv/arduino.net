#include "soft_debugger.h"


.section .text

.global DbgSaveRegisters

DbgSaveRegisters:
	cli
	push r31
	push r30
	push r29
	push r28
	push r24

	ldi	r30, lo8(__DbgSavedRegisters)
   	ldi	r31, hi8(__DbgSavedRegisters)
	ldi r28, 0		      			; Source: Y = position 32. Will pre-decrement later.
	ldi r29, 0		      			; Copying down

.loop:
	ld r24, Y+		      			; buffer source content in r27
	st Z+, r24		      			; Store the buffer content in *Z and increment Z
	cpi r28, 32
	brne .loop

	; Copy the stack pointer

	;ldi r27, 0x3e
	;st Z+, r27
	;ldi r27, 0x3d
	;st Z+, r27

	pop r24
	pop r28
	pop r29
	pop r30
	pop r31

	; Copy program counter from the stack to r1, r2

	;pop r1				  ; PC
	;pop r2				  ; PC
	;st Z+, r1
	;st Z+, r2
	;push r2 			  ; Restore PC in the stack
	;push r1 			  ; Restore PC in the stack

	; Recover r1 and r2 from the saved buffer

	;st -Z, r1
	;st -Z, r1
	;st -Z, r1
	;st -Z, r1
	;st -Z, r1
	;st -Z, r2

	sei
	ret

.end
