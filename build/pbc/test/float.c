#include "pbc.h"

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>
#include <stddef.h>

#include "readfile.h"

static void
dump(uint8_t *buffer, int sz) {
	int i , j;
	for (i=0;i<sz;i++) {
		printf("%02X ",buffer[i]);
		if (i % 16 == 15) {
			for (j = 0 ;j <16 ;j++) {
				char c = buffer[i/16 * 16+j];
				if (c>=32 && c<127) {
					printf("%c",c);
				} else {
					printf(".");
				}
			}
			printf("\n");
		}
	}

	printf("\n");
}

static void
test_rmessage(struct pbc_env *env, struct pbc_slice *slice) {
	struct pbc_rmessage * m = pbc_rmessage_new(env, "real", slice);
	printf("f = %f\n", pbc_rmessage_real(m , "f" , 0 ));
	printf("d = %f\n", pbc_rmessage_real(m , "d" , 0 ));
	pbc_rmessage_delete(m);
}

static struct pbc_wmessage *
test_wmessage(struct pbc_env * env)
{
	struct pbc_wmessage * msg = pbc_wmessage_new(env, "real");

	pbc_wmessage_real(msg, "f", 1.0);
	pbc_wmessage_real(msg, "d" , 4.0);

	return msg;
}

int
main()
{
	struct pbc_slice slice;
	read_file("float.pb", &slice);
	if (slice.buffer == NULL)
		return 1;
	struct pbc_env * env = pbc_new();
	pbc_register(env, &slice);

	free(slice.buffer);

	struct pbc_wmessage *msg = test_wmessage(env);

	pbc_wmessage_buffer(msg, &slice);

	dump(slice.buffer, slice.len);

	test_rmessage(env, &slice);

	pbc_wmessage_delete(msg);
	pbc_delete(env);

	return 0;
}
