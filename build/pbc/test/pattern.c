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

static struct pbc_pattern *pat;
static struct pbc_pattern *pat_phone;

struct person_phone {
	struct pbc_slice number;
	int32_t type;
};

struct person {
	struct pbc_slice name;
	int32_t id;
	struct pbc_slice email;
	pbc_array phone;
	pbc_array test;
};


static void
test_pattern_unpack(struct pbc_env *env, struct pbc_slice * slice) {
	struct person p;
	int r = pbc_pattern_unpack(pat, slice, &p);
	if (r>=0) {
		printf("name = %s\n",(const char *)p.name.buffer);
		printf("id = %d\n",p.id);
		printf("email = %s\n",(const char *)p.email.buffer);
		int n = pbc_array_size(p.phone);
		int i;
		for (i=0;i<n;i++) {
			struct pbc_slice * bytes = pbc_array_slice(p.phone, i);
			struct person_phone pp;
			pbc_pattern_unpack(pat_phone , bytes , &pp);
			printf("\tnumber = %s\n" , (const char*)pp.number.buffer);
			printf("\ttype = %d\n" , pp.type);
		}

		n = pbc_array_size(p.test);
		for (i=0;i<n;i++) {
			printf("test[%d] = %d\n",i, pbc_array_integer(p.test, i , NULL));
		}

		pbc_pattern_close_arrays(pat,&p);
	}
}

static int
test_pattern_pack(struct pbc_env *env , struct pbc_slice *slice) {
	struct person p;
	/*
	  If you don't care about default values (you will set all values by yourself) ,
      you can also use memset(&p, 0 , sizeof(p)) instead of pbc_pattern_set_default.
	*/
	pbc_pattern_set_default(pat, &p);

	p.name.buffer = (void*)"Alice";
	p.name.len = -1;	// encode '\0'
	p.id = 1234;
	p.email.buffer = (void*)"alice@unknown";
	p.email.len = -1;

	struct person_phone phone;
	phone.number.buffer = (void *)"1234567";
	phone.number.len = -1;
	phone.type = 1;

	char temp[128];		
	struct pbc_slice phone_slice = { temp, sizeof(temp) };

	int unused = pbc_pattern_pack(pat_phone, &phone , &phone_slice);
	
	if (unused < 0) {
		slice->len = 0;
		return slice->len;
	}

	pbc_array_push_slice(p.phone, &phone_slice);

	pbc_pattern_set_default(pat_phone, &phone);

	phone.number.buffer = (void *)"87654321";
	phone.number.len = -1;

	char temp2[128];		
	struct pbc_slice phone_slice2 = { temp2, sizeof(temp2) };

	unused = pbc_pattern_pack(pat_phone, &phone , &phone_slice2);
	
	if (unused < 0) {
		slice->len = 0;
		return slice->len;
	}

	pbc_array_push_slice(p.phone, &phone_slice2);

	int i;
	for (i=0;i<3;i++) {
		pbc_array_push_integer(p.test, -i*4,0);
	}

	int r = pbc_pattern_pack(pat, &p, slice);
	
	pbc_pattern_close_arrays(pat,&p);
	printf("pack into %d bytes\n", slice->len);

	return r;
}

int
main()
{
	struct pbc_slice slice;
	read_file("addressbook.pb", &slice);
	if (slice.buffer == NULL)
		return 1;
	struct pbc_env * env = pbc_new();
	pbc_register(env, &slice);

	free(slice.buffer);

	pat = pbc_pattern_new(env, "tutorial.Person" , 
		"name %s id %d email %s phone %a test %a",
		offsetof(struct person, name) , 
		offsetof(struct person, id) ,
		offsetof(struct person, email) ,
		offsetof(struct person, phone) ,
		offsetof(struct person, test));

	pat_phone = pbc_pattern_new(env, "tutorial.Person.PhoneNumber",
		"number %s type %d",
		offsetof(struct person_phone, number),
		offsetof(struct person_phone, type));


	char buffer[4096];
	struct pbc_slice message = { buffer, sizeof(buffer) };

	test_pattern_pack(env, &message);

	dump(message.buffer, message.len);

	test_pattern_unpack(env, &message);

	pbc_pattern_delete(pat);
	pbc_pattern_delete(pat_phone);

	pbc_delete(env);

	return 0;
}
