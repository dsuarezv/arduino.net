

.section .data

.global __DbgSavedRegisters

		__DbgSavedRegisters: .BYTE 36

.section .text

.extern DbgBreakImpl
.global DbgBreak

	DbgBreak:
		cli
		push r31
		push r30
		push r29
		push r28
		push r27

		ldi	r30, lo8(__DbgSavedRegisters)
       	ldi	r31, hi8(__DbgSavedRegisters)
		ldi r28, 32		      			; Source: Y = position 32. Will pre-decrement later.
		ldi r29, 0		      			; Copying down

    DbgSaveRegisters_loop:
		ld r27, -Y		      			; buffer source content in r27
		st Z+, r27		      			; Store the buffer content in *Z and increment Z
		cpi r28, 0
		brne DbgSaveRegisters_loop

		; Copy the stack pointer

		ldi r27, 0x3e
		st Z+, r27
		ldi r27, 0x3d
		st Z+, r27

		pop r27
		pop r28
		pop r29
		pop r30
		pop r31

		; Copy program counter from the stack to r1, r2

		pop r1				  ; PC
		pop r2				  ; PC
		st Z+, r1
		st Z+, r2
		push r2
		push r1

		; Recover r1 and r2 from the saved buffer

		st -Z, r1
		st -Z, r1
		st -Z, r1
		st -Z, r1
		st -Z, r1
		st -Z, r2

		sei
		call DbgBreakImpl

.end
