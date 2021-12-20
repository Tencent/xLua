#include <sys/time.h> // for gettimeofday()
extern "C" {
#include "lj_str_hash_x64.h"
}
#include <string>
#include <vector>
#include <utility>
#include <algorithm>
#include "test_util.hpp"
#include <stdio.h>
#include <math.h>

using namespace std;

#define lj_rol(x, n) (((x)<<(n)) | ((x)>>(-(int)(n)&(8*sizeof(x)-1))))
#define lj_ror(x, n) (((x)<<(-(int)(n)&(8*sizeof(x)-1))) | ((x)>>(n)))

const char* separator = "-------------------------------------------";

static uint32_t LJ_AINLINE
lj_original_hash(const char *str, size_t len)
{
  uint32_t a, b, h = len;
  if (len >= 4) {
    a = lj_getu32(str); h ^= lj_getu32(str+len-4);
    b = lj_getu32(str+(len>>1)-2);
    h ^= b; h -= lj_rol(b, 14);
    b += lj_getu32(str+(len>>2)-1);
    a ^= h; a -= lj_rol(h, 11);
    b ^= a; b -= lj_rol(a, 25);
    h ^= b; h -= lj_rol(b, 16);
  } else {
    a = *(const uint8_t *)str;
    h ^= *(const uint8_t *)(str+len-1);
    b = *(const uint8_t *)(str+(len>>1));
    h ^= b; h -= lj_rol(b, 14);
  }

  a ^= h; a -= lj_rol(h, 11);
  b ^= a; b -= lj_rol(a, 25);
  h ^= b; h -= lj_rol(b, 16);

  return h;
}

template<class T> double
BenchmarkHashTmpl(T func, char* buf, size_t len)
{
  TestClock timer;
  uint32_t h = 0;

  timer.start();
  for(int i = 1; i < 1000000 * 100; i++) {
    // So the buf is not loop invariant, hence the F(...)
    buf[i % 4096] = i;
    h += func(buf, len) ^ i;
  }
  timer.stop();

  // make h alive
  test_printf("%x", h);
  return timer.getElapseInSecond();
}

struct TestFuncWas
{
  uint32_t operator()(const char* buf, uint32_t len) {
    return lj_original_hash(buf, len);
  }
};

struct TestFuncIs
{
  uint32_t operator()(const char* buf, uint32_t len) {
    return lj_str_hash(buf, len);
  }
};

static void
benchmarkIndividual(char* buf)
{
  fprintf(stdout,"\n\nCompare performance of particular len (in second)\n");
  fprintf(stdout, "%-12s%-8s%-8s%s\n", "len", "was", "is", "diff");
  fprintf(stdout, "-------------------------------------------\n");

  uint32_t lens[] = {3, 4, 7, 10, 15, 16, 20, 32, 36, 63, 80, 100,
                     120, 127, 280, 290, 400};
  for (unsigned i = 0; i < sizeof(lens)/sizeof(lens[0]); i++) {
    uint32_t len = lens[i];
    double e1 = BenchmarkHashTmpl(TestFuncWas(), buf, len);
    double e2 = BenchmarkHashTmpl(TestFuncIs(), buf, len);
    fprintf(stdout, "len = %4d: %-7.3lf %-7.3lf %.2f\n", len, e1, e2, (e1-e2)/e1);
  }
}

template<class T> double
BenchmarkChangeLenTmpl(T func, char* buf, uint32_t* len_vect, uint32_t len_num)
{
  TestClock timer;
  uint32_t h = 0;

  timer.start();
  for(int i = 1; i < 1000000 * 100; i++) {
    for (int j = 0; j < (int)len_num; j++) {
      // So the buf is not loop invariant, hence the F(...)
      buf[(i + j) % 4096] = i;
      h += func(buf, len_vect[j]) ^ j;
    }
  }
  timer.stop();

  // make h alive
  test_printf("%x", h);
  return timer.getElapseInSecond();
}

// It is to measure the performance when length is changing.
// The purpose is to see how balanced branches impact the performance.
//
static void
benchmarkToggleLens(char* buf)
{
  double e1, e2;
  fprintf(stdout,"\nChanging length (in second):");
  fprintf(stdout, "\n%-20s%-8s%-8s%s\n%s\n", "len", "was", "is", "diff",
          separator);

  uint32_t lens1[] = {4, 9};
  e1 = BenchmarkChangeLenTmpl(TestFuncWas(), buf, lens1, 2);
  e2 = BenchmarkChangeLenTmpl(TestFuncIs(), buf, lens1, 2);
  fprintf(stdout, "%-20s%-7.3lf %-7.3lf %.2f\n", "4,9", e1, e2, (e1-e2)/e1);

  uint32_t lens2[] = {1, 4, 9};
  e1 = BenchmarkChangeLenTmpl(TestFuncWas(), buf, lens2, 3);
  e2 = BenchmarkChangeLenTmpl(TestFuncIs(), buf, lens2, 3);
  fprintf(stdout, "%-20s%-7.3lf %-7.3lf %.2f\n", "1,4,9", e1, e2, (e1-e2)/e1);

  uint32_t lens3[] = {1, 33, 4, 9};
  e1 = BenchmarkChangeLenTmpl(TestFuncWas(), buf, lens3, 4);
  e2 = BenchmarkChangeLenTmpl(TestFuncIs(), buf, lens3, 4);
  fprintf(stdout, "%-20s%-7.3lf %-7.3lf %.2f\n", "1,33,4,9",
          e1, e2, (e1-e2)/e1);
}

static void
genRandomString(uint32_t min, uint32_t max,
                uint32_t num, vector<string>& result)
{
  double scale = (max - min) / (RAND_MAX + 1.0);
  result.clear();
  result.reserve(num);
  for (uint32_t i = 0; i < num; i++) {
    uint32_t len = (rand() * scale) + min;

    char* buf = new char[len];
    for (uint32_t l = 0; l < len; l++) {
      buf[l] = rand() % 255;
    }
    result.push_back(string(buf, len));
    delete[] buf;
  }
}

// Return the standard deviation of given array of number
static double
standarDeviation(const vector<uint32_t>& v)
{
  uint64_t total = 0;
  for (vector<uint32_t>::const_iterator i = v.begin(), e = v.end();
      i != e; ++i) {
    total += *i;
  }

  double avg = total / (double)v.size();
  double sd = 0;

  for (vector<uint32_t>::const_iterator i = v.begin(), e = v.end();
       i != e; ++i) {
    double t = avg - *i;
    sd = sd + t*t;
  }

  return sqrt(sd/v.size());
}

static pair<double, double>
benchmarkConflictHelper(uint32_t bucketNum, const vector<string>& strs)
{
  if (bucketNum & (bucketNum - 1)) {
    bucketNum = (1L << (log2_floor(bucketNum) + 1));
  }
  uint32_t mask = bucketNum - 1;

  vector<uint32_t> conflictWas(bucketNum);
  vector<uint32_t> conflictIs(bucketNum);

  conflictWas.resize(bucketNum);
  conflictIs.resize(bucketNum);

  for (vector<string>::const_iterator i = strs.begin(), e = strs.end();
       i != e; ++i) {
    uint32_t h1 = lj_original_hash(i->c_str(), i->size());
    uint32_t h2 = lj_str_hash(i->c_str(), i->size());

    conflictWas[h1 & mask]++;
    conflictIs[h2 & mask]++;
  }

#if 0
  std::sort(conflictWas.begin(), conflictWas.end(), std::greater<int>());
  std::sort(conflictIs.begin(), conflictIs.end(), std::greater<int>());

  fprintf(stderr, "%d %d %d %d vs %d %d %d %d\n",
          conflictWas[0], conflictWas[1], conflictWas[2], conflictWas[3],
          conflictIs[0], conflictIs[1], conflictIs[2], conflictIs[3]);
#endif

  return pair<double, double>(standarDeviation(conflictWas),
                              standarDeviation(conflictIs));
}

static void
benchmarkConflict()
{
  srand(time(0));

  float loadFactor[] = { 0.5f, 1.0f, 2.0f, 4.0f, 8.0f };
  int bucketNum[] = { 512, 1024, 2048, 4096, 8192, 16384};
  int lenRange[][2] = { {1,3}, {4, 15}, {16, 127}, {128, 1024}, {1, 1024}};

  fprintf(stdout,
          "\nBechmarking conflict (stand deviation of conflict)\n%s\n",
          separator);

  for (uint32_t k = 0; k < sizeof(lenRange)/sizeof(lenRange[0]); k++) {
    fprintf(stdout, "\nlen range from %d - %d\n", lenRange[k][0],
            lenRange[k][1]);
    fprintf(stdout, "%-10s %-12s %-10s %-10s diff\n%s\n",
            "bucket", "load-factor", "was", "is", separator);
    for (uint32_t i = 0; i < sizeof(bucketNum)/sizeof(bucketNum[0]); ++i) {
      for (uint32_t j = 0;
           j < sizeof(loadFactor)/sizeof(loadFactor[0]);
           ++j) {
        int strNum = bucketNum[i] * loadFactor[j];
        vector<string> strs(strNum);
        genRandomString(lenRange[k][0], lenRange[k][1], strNum, strs);

        pair<double, double> p;
        p = benchmarkConflictHelper(bucketNum[i], strs);
        fprintf(stdout, "%-10d %-12.2f %-10.2f %-10.2f %.2f\n",
                bucketNum[i], loadFactor[j], p.first, p.second,
                p.first - p.second);
      }
    }
  }
}

static void
benchmarkHashFunc()
{
  char buf[4096];
  char c = getpid() % 'a';
  for (int i = 0; i < (int)sizeof(buf); i++) {
    buf[i] = (c + i) % 255;
  }

  benchmarkConflict();
  benchmarkIndividual(buf);
  benchmarkToggleLens(buf);
}

int
main(int argc, char** argv)
{
  fprintf(stdout, "========================\nMicro benchmark...\n");
  benchmarkHashFunc();
  return 0;
}
