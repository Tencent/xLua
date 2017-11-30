#include "pbc.h"

#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include "readfile.h"

static void
decode_all(void *ud , int type, const char * typename , union pbc_value *v, int id, const char *key) {
	printf("%s : ", key ) ;
	switch(type & ~PBC_REPEATED) {
	case PBC_MESSAGE:
		printf("[%s]  -> \n" , typename);
		pbc_decode(ud, typename, &(v->s), decode_all, ud);
		printf("---------\n");
		break;
	case PBC_INT:
		printf("%d\n", (int)v->i.low);
		break;
	case PBC_REAL:
		printf("%lf\n", v->f);
		break;
	case PBC_BOOL:
		printf("<%s>\n", v->i.low ? "true" : "false");
		break;
	case PBC_ENUM:
		printf("[%s:%d]\n", v->e.name , v->e.id);
		break;
	case PBC_STRING: {
		char buffer[v->s.len+1];
		memcpy(buffer, v->s.buffer, v->s.len);
		buffer[v->s.len] = '\0';
		printf("\"%s\"\n", buffer);
		break;
	}
	case PBC_BYTES: {
		int i;
		uint8_t *buffer = v->s.buffer;
		for (i=0;i<v->s.len;i++) {
			printf("%02X ",buffer[i]);
		}
		printf("\n");
		break;
	}
	case PBC_INT64: {
		printf("0x%x%08x\n",v->i.hi, v->i.low);
		break;
	}
	case PBC_UINT:
		printf("%u\n",v->i.low);
		break;
	default:
		printf("!!! %d\n", type);
		break;
	}
}

void
test_decode(struct pbc_env * env , const char * pb)
{
	struct pbc_slice slice;
	read_file(pb, &slice);

	pbc_decode(env, "google.protobuf.FileDescriptorSet", &slice, decode_all , env);

	int ret = pbc_register(env, &slice);

	printf("Register %d\n",ret);

	free(slice.buffer);
}

int
main(int argc, char *argv[])
{
	struct pbc_env * env = pbc_new();

	test_decode(env,argv[1]);

	pbc_delete(env);


	return 0;
}
